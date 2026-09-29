namespace TicketWorld.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketWorld.Domain.Entities;

public class ArtistConfiguration : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.ToTable("artists");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(300).IsRequired();
        builder.HasIndex(a => a.Name);

        builder.Property(a => a.Slug).HasMaxLength(300).IsRequired();
        builder.HasIndex(a => a.Slug).IsUnique();

        builder.Property(a => a.Genre).HasMaxLength(100);
        builder.HasIndex(a => a.Genre);

        builder.Property(a => a.Country).HasMaxLength(100);

        builder.Property(a => a.SocialLinks)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.HasIndex(a => a.FollowerCount);
    }
}

public class ArtistFollowerConfiguration : IEntityTypeConfiguration<ArtistFollower>
{
    public void Configure(EntityTypeBuilder<ArtistFollower> builder)
    {
        builder.ToTable("artist_followers");

        builder.HasKey(af => new { af.UserId, af.ArtistId });

        builder.HasOne(af => af.User)
            .WithMany(u => u.FollowedArtists)
            .HasForeignKey(af => af.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(af => af.Artist)
            .WithMany(a => a.Followers)
            .HasForeignKey(af => af.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(af => af.ArtistId);
        builder.HasIndex(af => af.FollowedAt);
    }
}

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name).HasMaxLength(300).IsRequired();
        builder.HasIndex(v => v.Name);

        builder.Property(v => v.Address).IsRequired();
        builder.Property(v => v.City).HasMaxLength(100).IsRequired();
        builder.HasIndex(v => v.City);

        builder.Property(v => v.Country).HasMaxLength(100).HasDefaultValue("Thailand");
        builder.Property(v => v.PostalCode).HasMaxLength(20);

        builder.Property(v => v.Latitude).HasPrecision(10, 7);
        builder.Property(v => v.Longitude).HasPrecision(10, 7);

        builder.HasIndex(v => v.DeletedAt);
    }
}

public class SeatLayoutConfiguration : IEntityTypeConfiguration<SeatLayout>
{
    public void Configure(EntityTypeBuilder<SeatLayout> builder)
    {
        builder.ToTable("seat_layouts");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Name).HasMaxLength(200).IsRequired();
        builder.Property(sl => sl.LayoutJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.HasOne(sl => sl.Venue)
            .WithMany(v => v.SeatLayouts)
            .HasForeignKey(sl => sl.VenueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sl => sl.VenueId);
    }
}
