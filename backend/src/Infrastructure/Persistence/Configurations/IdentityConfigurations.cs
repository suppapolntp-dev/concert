namespace TicketWorld.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketWorld.Domain.Entities;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.Property(u => u.PreferredLang).HasMaxLength(5).HasDefaultValue("en");
        builder.Property(u => u.PreferredCurrency).HasMaxLength(3).HasDefaultValue("THB");
        builder.Property(u => u.Timezone).HasMaxLength(50).HasDefaultValue("Asia/Bangkok");

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(u => u.Role);

        builder.Property(u => u.AuthProvider)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.GoogleSub).HasMaxLength(100);
        builder.HasIndex(u => u.GoogleSub)
            .IsUnique()
            .HasFilter("google_sub IS NOT NULL");

        builder.HasIndex(u => u.IsBanned);
        builder.HasIndex(u => u.DeletedAt);
        builder.HasIndex(u => u.CreatedAt);
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(r => r.TokenHash).IsUnique();

        builder.Property(r => r.DeviceInfo).HasMaxLength(300);
        builder.Property(r => r.ReplacedByToken).HasMaxLength(128);

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ExpiresAt);
        builder.HasIndex(r => r.IsRevoked);
    }
}

public class OrganizerProfileConfiguration : IEntityTypeConfiguration<OrganizerProfile>
{
    public void Configure(EntityTypeBuilder<OrganizerProfile> builder)
    {
        builder.ToTable("organizer_profiles");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CompanyName).HasMaxLength(300).IsRequired();
        builder.HasIndex(o => o.CompanyName);

        builder.Property(o => o.Website).HasMaxLength(500);
        builder.Property(o => o.TaxId).HasMaxLength(50);
        builder.HasIndex(o => o.TaxId)
            .IsUnique()
            .HasFilter("tax_id IS NOT NULL");

        builder.Property(o => o.BankAccountNoEnc).IsRequired();
        builder.Property(o => o.BankName).HasMaxLength(100).IsRequired();
        builder.Property(o => o.BankAccountLast4).HasMaxLength(4).IsRequired();
        builder.Property(o => o.PayoutSchedule).HasMaxLength(20).HasDefaultValue("monthly");

        builder.Property(o => o.CommissionRate)
            .HasPrecision(5, 4)
            .HasDefaultValue(0.0500m);

        builder.HasOne(o => o.User)
            .WithOne(u => u.OrganizerProfile)
            .HasForeignKey<OrganizerProfile>(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(o => o.UserId).IsUnique();

        builder.HasOne(o => o.ApprovedByUser)
            .WithMany()
            .HasForeignKey(o => o.ApprovedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.IsApproved);
        builder.HasIndex(o => o.DeletedAt);
    }
}

public class OrganizerStaffConfiguration : IEntityTypeConfiguration<OrganizerStaff>
{
    public void Configure(EntityTypeBuilder<OrganizerStaff> builder)
    {
        builder.ToTable("organizer_staff");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Organizer)
            .WithMany(o => o.StaffMembers)
            .HasForeignKey(s => s.OrganizerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.User)
            .WithMany(u => u.OrganizerStaffAssignments)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Event)
            .WithMany()
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.InvitedByUser)
            .WithMany()
            .HasForeignKey(s => s.InvitedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.OrganizerId, s.UserId, s.EventId }).IsUnique();
        builder.HasIndex(s => s.OrganizerId);
        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.EventId);
    }
}
