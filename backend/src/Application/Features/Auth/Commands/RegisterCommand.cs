namespace TicketWorld.Application.Features.Auth.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    bool TwoFactorEnabled);

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    string? Phone = null) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255);

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(v => v.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200);
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (exists)
            throw new ConflictException($"User with email '{normalizedEmail}' already exists.");

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(
            email: normalizedEmail,
            passwordHash: passwordHash,
            fullName: request.FullName,
            phone: request.Phone,
            role: UserRole.Customer,
            authProvider: AuthProvider.Local);

        _context.Users.Add(user);

        // Generate Tokens
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
