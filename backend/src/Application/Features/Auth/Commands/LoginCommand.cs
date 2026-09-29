namespace TicketWorld.Application.Features.Auth.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Exceptions;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null) : IRequest<AuthResponse>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (user == null)
            throw new NotFoundException("Invalid email or password.");

        if (user.IsBanned)
            throw new ForbiddenException($"Your account has been suspended: {user.BannedReason}");

        if (user.IsLockedOut())
            throw new ForbiddenException($"Account is temporarily locked due to multiple failed login attempts until {user.LockedUntil:O}.");

        var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isValidPassword)
        {
            user.RecordFailedLogin(maxFailedAttempts: 5, lockoutDuration: TimeSpan.FromMinutes(15));
            await _context.SaveChangesAsync(cancellationToken);
            throw new BusinessRuleException("Invalid email or password.");
        }

        // Reset failed counter on successful login
        user.ResetFailedLogin();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var hashedRefreshToken = _tokenService.HashToken(rawRefreshToken);
        var refreshExpires = DateTime.UtcNow.AddDays(7);

        var refreshToken = RefreshToken.Create(
            userId: user.Id,
            tokenHash: hashedRefreshToken,
            expiresAt: refreshExpires);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role, user.TwoFactorEnabled);
        return new AuthResponse(accessToken, rawRefreshToken, refreshExpires, userDto);
    }
}
