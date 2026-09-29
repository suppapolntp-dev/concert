namespace TicketWorld.Domain.Entities;

using System.Text.RegularExpressions;
using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Aggregate root representing a musical artist, band, or public performer.
/// Mapped to the 'artists' table in PostgreSQL.
/// </summary>
public class Artist : AggregateRoot, IAuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Bio { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string? Genre { get; private set; }
    public string? Country { get; private set; }
    public string SocialLinks { get; private set; } = "{}";
    public int FollowerCount { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<ArtistFollower> Followers { get; private set; } = new List<ArtistFollower>();
    public virtual ICollection<Event> Events { get; private set; } = new List<Event>();

    // Parameterless constructor for EF Core
    protected Artist() { }

    public static Artist Create(
        string name,
        string? slug = null,
        string? bio = null,
        string? genre = null,
        string? country = null,
        string? profileImageUrl = null,
        string? coverImageUrl = null,
        string? socialLinks = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Artist name is required.");

        var generatedSlug = string.IsNullOrWhiteSpace(slug)
            ? GenerateSlug(name)
            : slug.Trim().ToLowerInvariant();

        return new Artist
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = generatedSlug,
            Bio = bio?.Trim(),
            Genre = genre?.Trim(),
            Country = country?.Trim(),
            ProfileImageUrl = profileImageUrl?.Trim(),
            CoverImageUrl = coverImageUrl?.Trim(),
            SocialLinks = string.IsNullOrWhiteSpace(socialLinks) ? "{}" : socialLinks.Trim(),
            FollowerCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(
        string name,
        string? bio,
        string? genre,
        string? country,
        string? profileImageUrl,
        string? coverImageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Artist name cannot be empty.");

        Name = name.Trim();
        Bio = bio?.Trim();
        Genre = genre?.Trim();
        Country = country?.Trim();
        ProfileImageUrl = profileImageUrl?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSocialLinks(string socialLinksJson)
    {
        SocialLinks = string.IsNullOrWhiteSpace(socialLinksJson) ? "{}" : socialLinksJson.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementFollowerCount()
    {
        FollowerCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementFollowerCount()
    {
        if (FollowerCount > 0)
        {
            FollowerCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    private static string GenerateSlug(string text)
    {
        var str = text.Trim().ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(str) ? Guid.NewGuid().ToString("n") : str;
    }
}
