namespace TicketWorld.Application.Features.Auth.Queries;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Application.Features.Auth.Commands;
using TicketWorld.Domain.Exceptions;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new NotFoundException("User not found.");

        return new UserDto(user.Id, user.Email, user.FullName, user.Role, user.TwoFactorEnabled);
    }
}
