namespace TicketWorld.Application.Common.Interfaces;

using TicketWorld.Domain.Entities;

/// <summary>
/// Cryptographic JWT and refresh token service abstraction.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}
