namespace TicketWorld.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketWorld.Domain.Entities;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BookingReference).HasMaxLength(30).IsRequired();
        builder.HasIndex(b => b.BookingReference).IsUnique();

        builder.Property(b => b.TotalAmount).HasPrecision(12, 2).IsRequired();
        builder.Property(b => b.DiscountAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(b => b.FinalAmount).HasPrecision(12, 2).IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(b => b.Status);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Event)
            .WithMany(e => e.Bookings)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.PromoCode)
            .WithMany()
            .HasForeignKey(b => b.PromoCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.EventId);
        builder.HasIndex(b => b.ExpiresAt);
        builder.HasIndex(b => b.CreatedAt);
    }
}

public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
{
    public void Configure(EntityTypeBuilder<BookingItem> builder)
    {
        builder.ToTable("booking_items");

        builder.HasKey(bi => bi.Id);

        builder.Property(bi => bi.UnitPrice).HasPrecision(12, 2).IsRequired();
        builder.Property(bi => bi.QrCodePayload).IsRequired();

        builder.HasOne(bi => bi.Booking)
            .WithMany(b => b.Items)
            .HasForeignKey(bi => bi.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bi => bi.Seat)
            .WithMany()
            .HasForeignKey(bi => bi.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bi => bi.Tier)
            .WithMany(tt => tt.BookingItems)
            .HasForeignKey(bi => bi.TierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bi => bi.CheckedInByUser)
            .WithMany()
            .HasForeignKey(bi => bi.CheckedInBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(bi => bi.BookingId);
        builder.HasIndex(bi => bi.SeatId);
        builder.HasIndex(bi => bi.TierId);
        builder.HasIndex(bi => bi.IsCheckedIn);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PaymentIntentId).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.PaymentIntentId).IsUnique();

        builder.Property(p => p.IdempotencyKey).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.IdempotencyKey).IsUnique();

        builder.Property(p => p.Gateway)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(p => p.Status);

        builder.Property(p => p.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("THB");
        builder.Property(p => p.PaymentMethodType).HasMaxLength(50);

        builder.HasOne(p => p.Booking)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.BookingId);
    }
}

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("promo_codes");

        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(pc => pc.Code).IsUnique();

        builder.Property(pc => pc.DiscountType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(pc => pc.DiscountValue).HasPrecision(12, 2).IsRequired();
        builder.Property(pc => pc.MinOrderAmount).HasPrecision(12, 2).HasDefaultValue(0m);
        builder.Property(pc => pc.MaxDiscountAmount).HasPrecision(12, 2);

        builder.HasOne(pc => pc.Organizer)
            .WithMany()
            .HasForeignKey(pc => pc.OrganizerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(pc => pc.Event)
            .WithMany()
            .HasForeignKey(pc => pc.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(pc => pc.OrganizerId);
        builder.HasIndex(pc => pc.EventId);
        builder.HasIndex(pc => pc.IsActive);
        builder.HasIndex(pc => pc.ValidUntil);
    }
}

public class PromoUsageConfiguration : IEntityTypeConfiguration<PromoUsage>
{
    public void Configure(EntityTypeBuilder<PromoUsage> builder)
    {
        builder.ToTable("promo_usages");

        builder.HasKey(pu => pu.Id);

        builder.Property(pu => pu.DiscountApplied).HasPrecision(12, 2).IsRequired();

        builder.HasOne(pu => pu.PromoCode)
            .WithMany(pc => pc.Usages)
            .HasForeignKey(pu => pu.PromoCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pu => pu.User)
            .WithMany()
            .HasForeignKey(pu => pu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pu => pu.Booking)
            .WithMany()
            .HasForeignKey(pu => pu.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pu => pu.PromoCodeId);
        builder.HasIndex(pu => pu.UserId);
        builder.HasIndex(pu => pu.BookingId);
    }
}

public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("payouts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("THB");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(p => p.Status);

        builder.Property(p => p.BankAccountSnapshotEnc).IsRequired();
        builder.Property(p => p.TransactionReference).HasMaxLength(100);

        builder.HasOne(p => p.Organizer)
            .WithMany(o => o.Payouts)
            .HasForeignKey(p => p.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ApprovedByUser)
            .WithMany()
            .HasForeignKey(p => p.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.OrganizerId);
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.ToTable("refunds");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(r => r.Currency).HasMaxLength(3).HasDefaultValue("THB");

        builder.Property(r => r.Reason)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.HasIndex(r => r.Status);

        builder.Property(r => r.GatewayRefundId).HasMaxLength(100);
        builder.Property(r => r.TransactionReference).HasMaxLength(100);

        builder.HasOne(r => r.Booking)
            .WithMany(b => b.Refunds)
            .HasForeignKey(r => r.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Payment)
            .WithMany(p => p.Refunds)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ApprovedByUser)
            .WithMany()
            .HasForeignKey(r => r.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.BookingId);
        builder.HasIndex(r => r.PaymentId);
    }
}
