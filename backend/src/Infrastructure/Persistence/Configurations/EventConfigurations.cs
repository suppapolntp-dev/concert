namespace TicketWorld.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketWorld.Domain.Entities;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(300).IsRequired();
        builder.HasIndex(e => e.Title);

        builder.Property(e => e.Slug).HasMaxLength(300).IsRequired();
        builder.HasIndex(e => e.Slug).IsUnique();

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(e => e.Status);

        builder.Property(e => e.MaxTicketsPerUser).HasDefaultValue(4);

        builder.HasOne(e => e.Organizer)
            .WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Venue)
            .WithMany(v => v.Events)
            .HasForeignKey(e => e.VenueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SeatLayout)
            .WithMany(sl => sl.Events)
            .HasForeignKey(e => e.SeatLayoutId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.OrganizerId);
        builder.HasIndex(e => e.VenueId);
        builder.HasIndex(e => e.StartAt);
        builder.HasIndex(e => e.SalesStartAt);
        builder.HasIndex(e => e.DeletedAt);
    }
}

public class TicketTierConfiguration : IEntityTypeConfiguration<TicketTier>
{
    public void Configure(EntityTypeBuilder<TicketTier> builder)
    {
        builder.ToTable("ticket_tiers");

        builder.HasKey(tt => tt.Id);

        builder.Property(tt => tt.Name).HasMaxLength(100).IsRequired();

        builder.Property(tt => tt.TierType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(tt => tt.Price).HasPrecision(12, 2).IsRequired();
        builder.Property(tt => tt.ColorHex).HasMaxLength(10).HasDefaultValue("#3B82F6");
        builder.Property(tt => tt.EntryGate).HasMaxLength(50);

        builder.HasOne(tt => tt.Event)
            .WithMany(e => e.TicketTiers)
            .HasForeignKey(tt => tt.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(tt => tt.EventId);
    }
}

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("seats");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Section).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Row).HasMaxLength(20).IsRequired();
        builder.Property(s => s.SeatNumber).HasMaxLength(20).IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.PaymentIntentId).HasMaxLength(100);

        // PostgreSQL xmin Optimistic Concurrency token
        builder.Property(s => s.Version).IsRowVersion();

        builder.HasOne(s => s.Event)
            .WithMany(e => e.Seats)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Tier)
            .WithMany(tt => tt.Seats)
            .HasForeignKey(s => s.TierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.HeldByUser)
            .WithMany()
            .HasForeignKey(s => s.HeldByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique constraint: one physical seat coordinate per event
        builder.HasIndex(s => new { s.EventId, s.Section, s.Row, s.SeatNumber }).IsUnique();
        builder.HasIndex(s => new { s.EventId, s.Status });
        builder.HasIndex(s => s.HeldUntil);
        builder.HasIndex(s => s.PaymentIntentId);
    }
}
