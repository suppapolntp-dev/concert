namespace TicketWorld.Application.Common.Interfaces;

/// <summary>
/// Password hashing and verification abstraction using Argon2id.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
