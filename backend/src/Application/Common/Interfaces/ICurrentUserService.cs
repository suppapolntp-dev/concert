namespace TicketWorld.Application.Common.Interfaces;

using TicketWorld.Domain.Enums;

/// <summary>
/// Current authenticated user identity accessor.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
