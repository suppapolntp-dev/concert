using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketWorld.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    Genre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SocialLinks = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    FollowerCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_configs",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValueJson = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configs", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    PreferredLang = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "en"),
                    PreferredCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "THB"),
                    Timezone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Asia/Bangkok"),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    BannedReason = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    FailedLoginCount = table.Column<int>(type: "integer", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AuthProvider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GoogleSub = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "venues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Thailand"),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    TotalCapacity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Gateway = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EventId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EventType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "artist_followers",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtistId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artist_followers", x => new { x.UserId, x.ArtistId });
                    table.ForeignKey(
                        name: "FK_artist_followers_artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_artist_followers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    NewValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_ActorId",
                        column: x => x.ActorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "organizer_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CompanyLogoUrl = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankAccountNoEnc = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BankAccountLast4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    PayoutSchedule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "monthly"),
                    CommissionRate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false, defaultValue: 0.0500m),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizer_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organizer_profiles_users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_organizer_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DeviceInfo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    PolicyVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_consents_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seat_layouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LayoutJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seat_layouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seat_layouts_venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "THB"),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BankAccountSnapshotEnc = table.Column<string>(type: "text", nullable: false),
                    TransactionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payouts_organizer_profiles_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "organizer_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payouts_users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatLayoutId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PosterImageUrl = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DoorsOpenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SalesStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SalesEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxTicketsPerUser = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    SoldSeats = table.Column<int>(type: "integer", nullable: false),
                    HeldSeats = table.Column<int>(type: "integer", nullable: false),
                    HasWaitingRoom = table.Column<bool>(type: "boolean", nullable: false),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArtistId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_events_artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "artists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_events_organizer_profiles_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "organizer_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_events_seat_layouts_SeatLayoutId",
                        column: x => x.SeatLayoutId,
                        principalTable: "seat_layouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_events_venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organizer_staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Permissions = table.Column<List<string>>(type: "text[]", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    InvitedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizer_staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organizer_staff_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizer_staff_organizer_profiles_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "organizer_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizer_staff_users_InvitedBy",
                        column: x => x.InvitedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organizer_staff_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promo_codes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizerId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: true),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    MaxUses = table.Column<int>(type: "integer", nullable: false),
                    UsedCount = table.Column<int>(type: "integer", nullable: false),
                    MaxDiscountAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promo_codes_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_promo_codes_organizer_profiles_OrganizerId",
                        column: x => x.OrganizerId,
                        principalTable: "organizer_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ticket_tiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TierType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ColorHex = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "#3B82F6"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    TotalQuota = table.Column<int>(type: "integer", nullable: false),
                    AvailableQuota = table.Column<int>(type: "integer", nullable: false),
                    HeldQuota = table.Column<int>(type: "integer", nullable: false),
                    SoldQuota = table.Column<int>(type: "integer", nullable: false),
                    EntryGate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_tiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ticket_tiers_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "waiting_room_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    PriorityScore = table.Column<double>(type: "double precision", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waiting_room_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_waiting_room_tokens_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_waiting_room_tokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingReference = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromoCodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    FinalAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bookings_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bookings_promo_codes_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "promo_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bookings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "seats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Section = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Row = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SeatNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HeldByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeldUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentIntentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_seats_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_seats_ticket_tiers_TierId",
                        column: x => x.TierId,
                        principalTable: "ticket_tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_seats_users_HeldByUserId",
                        column: x => x.HeldByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "waitlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TierId = table.Column<Guid>(type: "uuid", nullable: true),
                    QueueNumber = table.Column<int>(type: "integer", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waitlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_waitlists_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_waitlists_ticket_tiers_TierId",
                        column: x => x.TierId,
                        principalTable: "ticket_tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_waitlists_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentIntentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Gateway = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "THB"),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PaymentMethodType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promo_usages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromoCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscountApplied = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_usages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promo_usages_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promo_usages_promo_codes_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "promo_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promo_usages_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reviews_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reviews_events_EventId",
                        column: x => x.EventId,
                        principalTable: "events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reviews_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "booking_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    TierId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    QrCodePayload = table.Column<string>(type: "text", nullable: false),
                    IsCheckedIn = table.Column<bool>(type: "boolean", nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckedInBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_items_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_items_seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_items_ticket_tiers_TierId",
                        column: x => x.TierId,
                        principalTable: "ticket_tiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_items_users_CheckedInBy",
                        column: x => x.CheckedInBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "THB"),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GatewayRefundId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RejectionReason = table.Column<string>(type: "text", nullable: true),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refunds_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_refunds_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_refunds_users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_refunds_users_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ticket_transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TransferToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ticket_transfers_booking_items_BookingItemId",
                        column: x => x.BookingItemId,
                        principalTable: "booking_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_transfers_users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ticket_transfers_users_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_artist_followers_ArtistId",
                table: "artist_followers",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_artist_followers_FollowedAt",
                table: "artist_followers",
                column: "FollowedAt");

            migrationBuilder.CreateIndex(
                name: "IX_artists_FollowerCount",
                table: "artists",
                column: "FollowerCount");

            migrationBuilder.CreateIndex(
                name: "IX_artists_Genre",
                table: "artists",
                column: "Genre");

            migrationBuilder.CreateIndex(
                name: "IX_artists_Name",
                table: "artists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_artists_Slug",
                table: "artists",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Action",
                table: "audit_logs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ActorId",
                table: "audit_logs",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAt",
                table: "audit_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityName",
                table: "audit_logs",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_booking_items_BookingId",
                table: "booking_items",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_items_CheckedInBy",
                table: "booking_items",
                column: "CheckedInBy");

            migrationBuilder.CreateIndex(
                name: "IX_booking_items_IsCheckedIn",
                table: "booking_items",
                column: "IsCheckedIn");

            migrationBuilder.CreateIndex(
                name: "IX_booking_items_SeatId",
                table: "booking_items",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_items_TierId",
                table: "booking_items",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_BookingReference",
                table: "bookings",
                column: "BookingReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bookings_CreatedAt",
                table: "bookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_EventId",
                table: "bookings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_ExpiresAt",
                table: "bookings",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_PromoCodeId",
                table: "bookings",
                column: "PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_Status",
                table: "bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_UserId",
                table: "bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_events_ArtistId",
                table: "events",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_events_DeletedAt",
                table: "events",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_events_OrganizerId",
                table: "events",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_events_SalesStartAt",
                table: "events",
                column: "SalesStartAt");

            migrationBuilder.CreateIndex(
                name: "IX_events_SeatLayoutId",
                table: "events",
                column: "SeatLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_events_Slug",
                table: "events",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_StartAt",
                table: "events",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_events_Status",
                table: "events",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_events_Title",
                table: "events",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_events_VenueId",
                table: "events",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_profiles_ApprovedBy",
                table: "organizer_profiles",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_profiles_CompanyName",
                table: "organizer_profiles",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_profiles_DeletedAt",
                table: "organizer_profiles",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_profiles_IsApproved",
                table: "organizer_profiles",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_profiles_TaxId",
                table: "organizer_profiles",
                column: "TaxId",
                unique: true,
                filter: "tax_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_profiles_UserId",
                table: "organizer_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizer_staff_EventId",
                table: "organizer_staff",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_staff_InvitedBy",
                table: "organizer_staff",
                column: "InvitedBy");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_staff_OrganizerId",
                table: "organizer_staff",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_organizer_staff_OrganizerId_UserId_EventId",
                table: "organizer_staff",
                columns: new[] { "OrganizerId", "UserId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organizer_staff_UserId",
                table: "organizer_staff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_BookingId",
                table: "payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_IdempotencyKey",
                table: "payments",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_PaymentIntentId",
                table: "payments",
                column: "PaymentIntentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_Status",
                table: "payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_payouts_ApprovedBy",
                table: "payouts",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_payouts_OrganizerId",
                table: "payouts",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_payouts_Status",
                table: "payouts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_promo_codes_Code",
                table: "promo_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promo_codes_EventId",
                table: "promo_codes",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_promo_codes_IsActive",
                table: "promo_codes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_promo_codes_OrganizerId",
                table: "promo_codes",
                column: "OrganizerId");

            migrationBuilder.CreateIndex(
                name: "IX_promo_codes_ValidUntil",
                table: "promo_codes",
                column: "ValidUntil");

            migrationBuilder.CreateIndex(
                name: "IX_promo_usages_BookingId",
                table: "promo_usages",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_promo_usages_PromoCodeId",
                table: "promo_usages",
                column: "PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_promo_usages_UserId",
                table: "promo_usages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ExpiresAt",
                table: "refresh_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_IsRevoked",
                table: "refresh_tokens",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_TokenHash",
                table: "refresh_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_ApprovedBy",
                table: "refunds",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_BookingId",
                table: "refunds",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_PaymentId",
                table: "refunds",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_RequestedBy",
                table: "refunds",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_Status",
                table: "refunds",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_BookingId",
                table: "reviews",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_CreatedAt",
                table: "reviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_EventId_IsApproved",
                table: "reviews",
                columns: new[] { "EventId", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_UserId",
                table: "reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_seat_layouts_VenueId",
                table: "seat_layouts",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_seats_EventId_Section_Row_SeatNumber",
                table: "seats",
                columns: new[] { "EventId", "Section", "Row", "SeatNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seats_EventId_Status",
                table: "seats",
                columns: new[] { "EventId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_seats_HeldByUserId",
                table: "seats",
                column: "HeldByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_seats_HeldUntil",
                table: "seats",
                column: "HeldUntil");

            migrationBuilder.CreateIndex(
                name: "IX_seats_PaymentIntentId",
                table: "seats",
                column: "PaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_seats_TierId",
                table: "seats",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_tiers_EventId",
                table: "ticket_tiers",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_transfers_BookingItemId",
                table: "ticket_transfers",
                column: "BookingItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_transfers_ExpiresAt",
                table: "ticket_transfers",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_transfers_FromUserId",
                table: "ticket_transfers",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_transfers_Status",
                table: "ticket_transfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_transfers_ToUserId",
                table: "ticket_transfers",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_transfers_TransferToken",
                table: "ticket_transfers",
                column: "TransferToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_consents_CreatedAt",
                table: "user_consents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_user_consents_UserId_ConsentType",
                table: "user_consents",
                columns: new[] { "UserId", "ConsentType" });

            migrationBuilder.CreateIndex(
                name: "IX_users_CreatedAt",
                table: "users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_users_DeletedAt",
                table: "users",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_GoogleSub",
                table: "users",
                column: "GoogleSub",
                unique: true,
                filter: "google_sub IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_IsBanned",
                table: "users",
                column: "IsBanned");

            migrationBuilder.CreateIndex(
                name: "IX_users_Role",
                table: "users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_venues_City",
                table: "venues",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_venues_DeletedAt",
                table: "venues",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_venues_Name",
                table: "venues",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_waiting_room_tokens_EventId",
                table: "waiting_room_tokens",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_waiting_room_tokens_ExpiresAt",
                table: "waiting_room_tokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_waiting_room_tokens_TokenHash",
                table: "waiting_room_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_waiting_room_tokens_UserId",
                table: "waiting_room_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_waitlists_EventId_Status_QueueNumber",
                table: "waitlists",
                columns: new[] { "EventId", "Status", "QueueNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_waitlists_EventId_UserId_TierId",
                table: "waitlists",
                columns: new[] { "EventId", "UserId", "TierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_waitlists_TierId",
                table: "waitlists",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_waitlists_UserId",
                table: "waitlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_CreatedAt",
                table: "webhook_events",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_Gateway_EventId",
                table: "webhook_events",
                columns: new[] { "Gateway", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_IsProcessed",
                table: "webhook_events",
                column: "IsProcessed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artist_followers");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "organizer_staff");

            migrationBuilder.DropTable(
                name: "payouts");

            migrationBuilder.DropTable(
                name: "promo_usages");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "system_configs");

            migrationBuilder.DropTable(
                name: "ticket_transfers");

            migrationBuilder.DropTable(
                name: "user_consents");

            migrationBuilder.DropTable(
                name: "waiting_room_tokens");

            migrationBuilder.DropTable(
                name: "waitlists");

            migrationBuilder.DropTable(
                name: "webhook_events");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "booking_items");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "seats");

            migrationBuilder.DropTable(
                name: "promo_codes");

            migrationBuilder.DropTable(
                name: "ticket_tiers");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "artists");

            migrationBuilder.DropTable(
                name: "organizer_profiles");

            migrationBuilder.DropTable(
                name: "seat_layouts");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "venues");
        }
    }
}
