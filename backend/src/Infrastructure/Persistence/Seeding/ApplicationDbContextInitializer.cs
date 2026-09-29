namespace TicketWorld.Infrastructure.Persistence.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;

public class ApplicationDbContextInitializer : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;

    public ApplicationDbContextInitializer(
        ApplicationDbContext context,
        IPasswordHasher _passwordHasher,
        ILogger<ApplicationDbContextInitializer> logger)
    {
        _context = context;
        this._passwordHasher = _passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SeedUsersAndOrganizersAsync(cancellationToken);
            await SeedCatalogAndEventsAsync(cancellationToken);
            await SeedSystemConfigsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding database master data.");
            throw;
        }
    }

    private async Task SeedUsersAndOrganizersAsync(CancellationToken cancellationToken)
    {
        // 1. Default Admin
        var adminEmail = "admin@ticketworld.com";
        var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail, cancellationToken);
        if (existingAdmin == null)
        {
            var adminUser = User.Create(
                email: adminEmail,
                passwordHash: _passwordHasher.HashPassword("Admin@TicketWorld2026!"),
                fullName: "TicketWorld Administrator",
                phone: "+66812345678",
                role: UserRole.Admin);

            _context.Users.Add(adminUser);
            _logger.LogInformation("Seeded default Admin user: {Email}", adminEmail);
        }

        // 2. Default Organizer
        var organizerEmail = "organizer@ticketworld.com";
        var existingOrganizer = await _context.Users.FirstOrDefaultAsync(u => u.Email == organizerEmail, cancellationToken);
        if (existingOrganizer == null)
        {
            var organizerUser = User.Create(
                email: organizerEmail,
                passwordHash: _passwordHasher.HashPassword("Organizer@TicketWorld2026!"),
                fullName: "Live Nation Tero Entertainment",
                phone: "+66898765432",
                role: UserRole.Organizer);

            _context.Users.Add(organizerUser);

            var profile = OrganizerProfile.Create(
                userId: organizerUser.Id,
                companyName: "Live Nation Tero Co., Ltd.",
                bankAccountNoEnc: "enc_bank_1234567890",
                bankName: "Kasikornbank",
                bankAccountLast4: "7890",
                taxId: "0105541012345",
                description: "Leading concert and live entertainment organizer in Southeast Asia.",
                website: "https://livenationtero.co.th");

            profile.Approve(organizerUser.Id);
            _context.OrganizerProfiles.Add(profile);

            _logger.LogInformation("Seeded default Organizer user and profile: {Email}", organizerEmail);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCatalogAndEventsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Venues.AnyAsync(cancellationToken))
            return;

        // 1. Venue
        var venue = Venue.Create(
            name: "Impact Arena Muang Thong Thani",
            address: "Popular 3 Rd, Ban Mai, Pak Kret District",
            city: "Nonthaburi",
            totalCapacity: 12000,
            country: "Thailand",
            postalCode: "11120",
            latitude: 13.9114m,
            longitude: 100.5489m);

        _context.Venues.Add(venue);

        // 2. Seat Layout
        var layout = SeatLayout.Create(
            venueId: venue.Id,
            name: "Concert Arena Standard 3-Tier Layout",
            layoutJson: "{\"sections\": [\"VIP\", \"A\", \"B\"]}",
            totalSeats: 30,
            isDefault: true);

        _context.SeatLayouts.Add(layout);

        // 3. Organizer Profile lookup
        var organizer = await _context.OrganizerProfiles.FirstOrDefaultAsync(cancellationToken);
        if (organizer == null) return;

        // 4. Sample Concert Event
        var ev = Event.Create(
            organizerId: organizer.Id,
            venueId: venue.Id,
            seatLayoutId: layout.Id,
            title: "Coldplay: Music of the Spheres Tour Bangkok 2026",
            slug: "coldplay-music-of-the-spheres-bangkok-2026",
            description: "Coldplay live in Bangkok at Impact Arena with breathtaking lights and sustainable music experience.",
            category: EventCategory.Concert,
            startAt: DateTime.UtcNow.AddMonths(2),
            endAt: DateTime.UtcNow.AddMonths(2).AddHours(4),
            salesStartAt: DateTime.UtcNow.AddDays(-1),
            salesEndAt: DateTime.UtcNow.AddMonths(2),
            maxTicketsPerUser: 4,
            hasWaitingRoom: true,
            posterImageUrl: "/uploads/posters/coldplay-2026.jpg");

        ev.Publish();
        ev.StartSales();
        _context.Events.Add(ev);

        // 5. Ticket Tiers
        var vipTier = TicketTier.Create(ev.Id, "VIP Standing Front", 6500m, 10, TicketTierType.Standing, "#EAB308", "Front stage access + merchandise bundle", "Gate 1");
        var cat1Tier = TicketTier.Create(ev.Id, "Cat 1 Seated Lower", 4500m, 10, TicketTierType.Standard, "#3B82F6", "Lower tier prime viewing seats", "Gate 2");
        var cat2Tier = TicketTier.Create(ev.Id, "Cat 2 Seated Upper", 2500m, 10, TicketTierType.Economy, "#10B981", "Upper tier panoramic view", "Gate 3");

        _context.TicketTiers.AddRange(vipTier, cat1Tier, cat2Tier);

        // 6. Generate 30 Sample Seats
        var seats = new List<Seat>();

        for (int i = 1; i <= 10; i++)
        {
            seats.Add(Seat.Create(ev.Id, vipTier.Id, "VIP", "1", i.ToString()));
        }

        for (int i = 1; i <= 10; i++)
        {
            seats.Add(Seat.Create(ev.Id, cat1Tier.Id, "A", "1", i.ToString()));
        }

        for (int i = 1; i <= 10; i++)
        {
            seats.Add(Seat.Create(ev.Id, cat2Tier.Id, "B", "1", i.ToString()));
        }

        _context.Seats.AddRange(seats);
        ev.UpdateDetails(
            ev.Title, ev.Description, ev.Category, ev.StartAt, ev.EndAt,
            ev.SalesStartAt, ev.SalesEndAt, ev.MaxTicketsPerUser, ev.HasWaitingRoom);

        // 7. Promo Code
        var promo = PromoCode.Create(
            code: "EARLYBIRD20",
            discountType: DiscountType.Percentage,
            discountValue: 20m,
            maxUses: 100,
            validFrom: DateTime.UtcNow.AddDays(-2),
            validUntil: DateTime.UtcNow.AddDays(30),
            minOrderAmount: 2000m,
            maxDiscountAmount: 2000m,
            organizerId: organizer.Id,
            eventId: ev.Id);

        _context.PromoCodes.Add(promo);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded venue, event, tiers, seats, and promo codes for {Title}", ev.Title);
    }

    private async Task SeedSystemConfigsAsync(CancellationToken cancellationToken)
    {
        var existingConfigs = await _context.SystemConfigs.ToDictionaryAsync(c => c.Id, cancellationToken);

        var defaultConfigs = new Dictionary<string, (string Value, string Description)>
        {
            ["platform_fee_percent"] = ("0.05", "Standard platform commission fee percentage (5%)"),
            ["ticket_hold_duration_minutes"] = ("10", "Duration in minutes a user can hold seats before release"),
            ["waiting_room_enabled"] = ("false", "Global kill-switch for virtual waiting room feature")
        };

        foreach (var (key, (val, desc)) in defaultConfigs)
        {
            if (!existingConfigs.ContainsKey(key))
            {
                var cfg = SystemConfig.Create(key, val, desc);
                _context.SystemConfigs.Add(cfg);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
