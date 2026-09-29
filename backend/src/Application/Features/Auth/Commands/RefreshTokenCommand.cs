namespace TicketWorld.Application.Features.Auth.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Exceptions;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(v => v.RefreshToken).NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hashedToken = _tokenService.HashToken(request.RefreshToken);

        var existingToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hashedToken, cancellationToken);

        if (existingToken == null || !existingToken.IsActive)
        {
            // Possible token reuse attack: if token was revoked, invalidate all user sessions
            if (existingToken != null && existingToken.IsRevoked)
            {
                var allUserTokens = await _context.RefreshTokens
                    .Where(r => r.UserId == existingToken.UserId && !r.IsRevoked)
                    .ToListAsync(cancellationToken);

                foreach (var t in allUserTokens)
                {
                    t.Revoke("Security.TokenReuseDetected");
                }
                await _context.SaveChangesAsync(cancellationToken);
            }

            throw new ForbiddenException("Invalid or expired refresh token.");
        }

        var user = existingToken.User;
        if (user.IsBanned)
            throw new ForbiddenException("Your account is currently suspended.");

        // Rotate token
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRawRefreshToken = _tokenService.GenerateRefreshToken();
        var newHashedToken = _tokenService.HashToken(newRawRefreshToken);
        var refreshExpires = DateTime.UtcNow.AddDays(7);

        existingToken.Revoke(newHashedToken);

        var newRefreshToken = RefreshToken.Create(
            userId: user.Id,
            tokenHash: newHashedToken,
            expiresAt: refreshExpires);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role, user.TwoFactorEnabled);
        return new AuthResponse(newAccessToken, newRawRefreshToken, refreshExpires, userDto);
    }
}
