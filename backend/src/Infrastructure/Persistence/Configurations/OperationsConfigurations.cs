namespace TicketWorld.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketWorld.Domain.Entities;

public class TicketTransferConfiguration : IEntityTypeConfiguration<TicketTransfer>
{
    public void Configure(EntityTypeBuilder<TicketTransfer> builder)
    {
        builder.ToTable("ticket_transfers");

        builder.HasKey(tt => tt.Id);

        builder.Property(tt => tt.ToEmail).HasMaxLength(255).IsRequired();
        builder.Property(tt => tt.TransferToken).HasMaxLength(128).IsRequired();
        builder.HasIndex(tt => tt.TransferToken).IsUnique();

        builder.Property(tt => tt.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(tt => tt.Status);

        builder.HasOne(tt => tt.BookingItem)
            .WithMany(bi => bi.Transfers)
            .HasForeignKey(tt => tt.BookingItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tt => tt.FromUser)
            .WithMany()
            .HasForeignKey(tt => tt.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tt => tt.ToUser)
            .WithMany()
            .HasForeignKey(tt => tt.ToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(tt => tt.BookingItemId);
        builder.HasIndex(tt => tt.FromUserId);
        builder.HasIndex(tt => tt.ToUserId);
        builder.HasIndex(tt => tt.ExpiresAt);
    }
}

public class WaitlistConfiguration : IEntityTypeConfiguration<Waitlist>
{
    public void Configure(EntityTypeBuilder<Waitlist> builder)
    {
        builder.ToTable("waitlists");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasOne(w => w.Event)
            .WithMany(e => e.Waitlists)
            .HasForeignKey(w => w.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.User)
            .WithMany(u => u.Waitlists)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Tier)
            .WithMany()
            .HasForeignKey(w => w.TierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(w => new { w.EventId, w.UserId, w.TierId }).IsUnique();
        builder.HasIndex(w => new { w.EventId, w.Status, w.QueueNumber });
    }
}

public class WaitingRoomTokenConfiguration : IEntityTypeConfiguration<WaitingRoomToken>
{
    public void Configure(EntityTypeBuilder<WaitingRoomToken> builder)
    {
        builder.ToTable("waiting_room_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.HasOne(t => t.Event)
            .WithMany()
            .HasForeignKey(t => t.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.EventId);
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.ExpiresAt);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.Action).HasMaxLength(100).IsRequired();
        builder.HasIndex(al => al.Action);

        builder.Property(al => al.EntityName).HasMaxLength(100).IsRequired();
        builder.HasIndex(al => al.EntityName);

        builder.Property(al => al.EntityId).HasMaxLength(100);

        builder.Property(al => al.OldValuesJson).HasColumnType("jsonb");
        builder.Property(al => al.NewValuesJson).HasColumnType("jsonb");

        builder.Property(al => al.IpAddress).HasMaxLength(50);
        builder.Property(al => al.UserAgent).HasMaxLength(500);

        builder.HasOne(al => al.Actor)
            .WithMany()
            .HasForeignKey(al => al.ActorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(al => al.ActorId);
        builder.HasIndex(al => al.CreatedAt);
    }
}

public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    public void Configure(EntityTypeBuilder<SystemConfig> builder)
    {
        builder.ToTable("system_configs");

        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id).HasColumnName("key").HasMaxLength(100);

        builder.Property(sc => sc.ValueJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(sc => sc.Description).HasMaxLength(500);
    }
}

public class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> builder)
    {
        builder.ToTable("user_consents");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.ConsentType).HasMaxLength(100).IsRequired();
        builder.Property(uc => uc.PolicyVersion).HasMaxLength(50).IsRequired();
        builder.Property(uc => uc.IpAddress).HasMaxLength(50);
        builder.Property(uc => uc.UserAgent).HasMaxLength(500);

        builder.HasOne(uc => uc.User)
            .WithMany(u => u.UserConsents)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(uc => new { uc.UserId, uc.ConsentType });
        builder.HasIndex(uc => uc.CreatedAt);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);

        builder.HasOne(r => r.Event)
            .WithMany(e => e.Reviews)
            .HasForeignKey(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Booking)
            .WithMany()
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.EventId, r.IsApproved });
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CreatedAt);
    }
}

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("webhook_events");

        builder.HasKey(we => we.Id);

        builder.Property(we => we.Gateway)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(we => we.EventId).HasMaxLength(150).IsRequired();
        builder.Property(we => we.EventType).HasMaxLength(150).IsRequired();

        builder.Property(we => we.PayloadJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(we => new { we.Gateway, we.EventId }).IsUnique();
        builder.HasIndex(we => we.IsProcessed);
        builder.HasIndex(we => we.CreatedAt);
    }
}
