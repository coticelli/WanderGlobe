using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WanderGlobe.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    ProfilePicture = table.Column<string>(type: "TEXT", nullable: true),
                    JoinDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Criteria = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Continent = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    FlagUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlannedTrips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CityName = table.Column<string>(type: "TEXT", nullable: false),
                    CountryName = table.Column<string>(type: "TEXT", nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CompletionPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedTrips", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DreamDestinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CityName = table.Column<string>(type: "TEXT", nullable: false),
                    CountryName = table.Column<string>(type: "TEXT", nullable: false),
                    CountryCode = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DreamDestinations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DreamDestinations_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    BadgeId = table.Column<int>(type: "INTEGER", nullable: false),
                    AchievedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => new { x.UserId, x.BadgeId });
                    table.ForeignKey(
                        name: "FK_UserBadges_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsCapital = table.Column<bool>(type: "INTEGER", nullable: false),
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: true),
                    Longitude = table.Column<double>(type: "REAL", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelJournals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelJournals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelJournals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TravelJournals_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitedCountries",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitedCountries", x => new { x.UserId, x.CountryId });
                    table.ForeignKey(
                        name: "FK_VisitedCountries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitedCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlannedTripId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_PlannedTrips_PlannedTripId",
                        column: x => x.PlannedTripId,
                        principalTable: "PlannedTrips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", nullable: false),
                    TravelJournalId = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_TravelJournals_TravelJournalId",
                        column: x => x.TravelJournalId,
                        principalTable: "TravelJournals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 1, "IT", "Europa", null, 41.902799999999999, 12.4964, "Italia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 2, "FR", "Europa", null, 48.8566, 2.3521999999999998, "Francia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 3, "US", "Nord America", null, 38.907200000000003, -77.036900000000003, "Stati Uniti" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 4, "DE", "Europa", null, 52.520000000000003, 13.404999999999999, "Germania" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 5, "ES", "Europa", null, 40.416800000000002, -3.7038000000000002, "Spagna" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 6, "PT", "Europa", null, 38.722299999999997, -9.1393000000000004, "Portogallo" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 7, "CH", "Europa", null, 46.948, 7.4474, "Svizzera" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 8, "AT", "Europa", null, 48.208199999999998, 16.373799999999999, "Austria" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 9, "BE", "Europa", null, 50.850299999999997, 4.3517000000000001, "Belgio" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 10, "NL", "Europa", null, 52.367600000000003, 4.9040999999999997, "Paesi Bassi" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 11, "GB", "Europa", null, 51.507399999999997, -0.1278, "Regno Unito" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 12, "CA", "Nord America", null, 45.421500000000002, -75.697199999999995, "Canada" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 13, "JP", "Asia", null, 35.676200000000001, 139.65029999999999, "Giappone" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 14, "CN", "Asia", null, 39.904200000000003, 116.4074, "Cina" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 15, "AU", "Oceania", null, -35.280900000000003, 149.13, "Australia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 16, "RU", "Europa/Asia", null, 55.755800000000001, 37.6173, "Russia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 17, "BR", "Sud America", null, -15.780099999999999, -47.929200000000002, "Brasile" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 18, "IN", "Asia", null, 28.613900000000001, 77.209000000000003, "India" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 19, "ZA", "Africa", null, -25.746099999999998, 28.188099999999999, "Sud Africa" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 20, "MX", "Nord America", null, 19.432600000000001, -99.133200000000002, "Messico" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 21, "AR", "Sud America", null, -34.603700000000003, -58.381599999999999, "Argentina" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 22, "GR", "Europa", null, 37.983800000000002, 23.727499999999999, "Grecia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 23, "EG", "Africa", null, 30.0444, 31.235700000000001, "Egitto" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 24, "SE", "Europa", null, 59.329300000000003, 18.0686, "Svezia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 25, "NO", "Europa", null, 59.913899999999998, 10.7522, "Norvegia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 26, "DK", "Europa", null, 55.676099999999998, 12.568300000000001, "Danimarca" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 27, "FI", "Europa", null, 60.169899999999998, 24.938400000000001, "Finlandia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 28, "IE", "Europa", null, 53.349800000000002, -6.2603, "Irlanda" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 29, "NZ", "Oceania", null, -41.286499999999997, 174.77619999999999, "Nuova Zelanda" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 30, "SG", "Asia", null, 1.3521000000000001, 103.8198, "Singapore" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 31, "TH", "Asia", null, 13.7563, 100.5018, "Thailandia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 32, "VN", "Asia", null, 21.027799999999999, 105.8342, "Vietnam" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 33, "ID", "Asia", null, -6.2088000000000001, 106.8456, "Indonesia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 34, "MY", "Asia", null, 3.1389999999999998, 101.68689999999999, "Malesia" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Code", "Continent", "FlagUrl", "Latitude", "Longitude", "Name" },
                values: new object[] { 35, "TR", "Europa/Asia", null, 39.933399999999999, 32.859699999999997, "Turchia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 1, 1, null, null, true, 41.902799999999999, 12.4964, "Roma" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 2, 2, null, null, true, 48.8566, 2.3521999999999998, "Parigi" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 3, 3, null, null, true, 38.907200000000003, -77.036900000000003, "Washington D.C." });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 4, 4, null, null, true, 52.520000000000003, 13.404999999999999, "Berlino" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 5, 5, null, null, true, 40.416800000000002, -3.7038000000000002, "Madrid" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 6, 6, null, null, true, 38.722299999999997, -9.1393000000000004, "Lisbona" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 7, 7, null, null, true, 46.948, 7.4474, "Berna" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 8, 8, null, null, true, 48.208199999999998, 16.373799999999999, "Vienna" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 9, 9, null, null, true, 50.850299999999997, 4.3517000000000001, "Bruxelles" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 10, 10, null, null, true, 52.367600000000003, 4.9040999999999997, "Amsterdam" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 11, 11, null, null, true, 51.507399999999997, -0.1278, "Londra" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 12, 12, null, null, true, 45.421500000000002, -75.697199999999995, "Ottawa" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 13, 13, null, null, true, 35.676200000000001, 139.65029999999999, "Tokyo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 14, 14, null, null, true, 39.904200000000003, 116.4074, "Pechino" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 15, 15, null, null, true, -35.280900000000003, 149.13, "Canberra" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 16, 16, null, null, true, 55.755800000000001, 37.6173, "Mosca" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 17, 17, null, null, true, -15.780099999999999, -47.929200000000002, "Brasilia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 18, 18, null, null, true, 28.613900000000001, 77.209000000000003, "Nuova Delhi" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 19, 19, null, null, true, -25.746099999999998, 28.188099999999999, "Pretoria" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 20, 20, null, null, true, 19.432600000000001, -99.133200000000002, "Città del Messico" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 21, 21, null, null, true, -34.603700000000003, -58.381599999999999, "Buenos Aires" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 22, 22, null, null, true, 37.983800000000002, 23.727499999999999, "Atene" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 23, 23, null, null, true, 30.0444, 31.235700000000001, "Il Cairo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 24, 24, null, null, true, 59.329300000000003, 18.0686, "Stoccolma" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 25, 25, null, null, true, 59.913899999999998, 10.7522, "Oslo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 26, 26, null, null, true, 55.676099999999998, 12.568300000000001, "Copenhagen" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 27, 27, null, null, true, 60.169899999999998, 24.938400000000001, "Helsinki" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 28, 28, null, null, true, 53.349800000000002, -6.2603, "Dublino" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 29, 29, null, null, true, -41.286499999999997, 174.77619999999999, "Wellington" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 30, 30, null, null, true, 1.3521000000000001, 103.8198, "Singapore" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 31, 31, null, null, true, 13.7563, 100.5018, "Bangkok" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 32, 32, null, null, true, 21.027799999999999, 105.8342, "Hanoi" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 33, 33, null, null, true, -6.2088000000000001, 106.8456, "Jakarta" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 34, 34, null, null, true, 3.1389999999999998, 101.68689999999999, "Kuala Lumpur" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 35, 35, null, null, true, 39.933399999999999, 32.859699999999997, "Ankara" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 36, 1, null, null, false, 45.464199999999998, 9.1899999999999995, "Milano" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 37, 1, null, null, false, 40.851799999999997, 14.2681, "Napoli" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 38, 1, null, null, false, 43.769599999999997, 11.255800000000001, "Firenze" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 39, 1, null, null, false, 45.440800000000003, 12.3155, "Venezia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 40, 1, null, null, false, 44.494900000000001, 11.342599999999999, "Bologna" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 41, 1, null, null, false, 45.070300000000003, 7.6868999999999996, "Torino" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 42, 1, null, null, false, 38.115699999999997, 13.361499999999999, "Palermo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 43, 2, null, null, false, 43.296500000000002, 5.3697999999999997, "Marsiglia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 44, 2, null, null, false, 45.764000000000003, 4.8357000000000001, "Lione" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 45, 2, null, null, false, 43.7102, 7.2619999999999996, "Nizza" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 46, 2, null, null, false, 44.837800000000001, -0.57920000000000005, "Bordeaux" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 47, 2, null, null, false, 43.604700000000001, 1.4441999999999999, "Tolosa" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 48, 2, null, null, false, 48.573399999999999, 7.7521000000000004, "Strasburgo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 49, 3, null, null, false, 40.712800000000001, -74.006, "New York" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 50, 3, null, null, false, 34.052199999999999, -118.2437, "Los Angeles" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 51, 3, null, null, false, 41.878100000000003, -87.629800000000003, "Chicago" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 52, 3, null, null, false, 25.761700000000001, -80.191800000000001, "Miami" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 53, 3, null, null, false, 37.774900000000002, -122.4194, "San Francisco" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 54, 3, null, null, false, 36.169899999999998, -115.13979999999999, "Las Vegas" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 55, 3, null, null, false, 42.360100000000003, -71.058899999999994, "Boston" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 56, 4, null, null, false, 48.135100000000001, 11.582000000000001, "Monaco" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 57, 4, null, null, false, 53.551099999999998, 9.9937000000000005, "Amburgo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 58, 4, null, null, false, 50.110900000000001, 8.6821000000000002, "Francoforte" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 59, 4, null, null, false, 50.9375, 6.9603000000000002, "Colonia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 60, 4, null, null, false, 51.227699999999999, 6.7735000000000003, "Düsseldorf" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 61, 5, null, null, false, 41.385100000000001, 2.1734, "Barcellona" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 62, 5, null, null, false, 39.469900000000003, -0.37630000000000002, "Valencia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 63, 5, null, null, false, 37.389099999999999, -5.9844999999999997, "Siviglia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 64, 5, null, null, false, 43.262999999999998, -2.9350000000000001, "Bilbao" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 65, 5, null, null, false, 36.721299999999999, -4.4212999999999996, "Malaga" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 66, 11, null, null, false, 53.480800000000002, -2.2425999999999999, "Manchester" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 67, 11, null, null, false, 52.486199999999997, -1.8904000000000001, "Birmingham" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 68, 11, null, null, false, 55.864199999999997, -4.2518000000000002, "Glasgow" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 69, 11, null, null, false, 53.4084, -2.9916, "Liverpool" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 70, 11, null, null, false, 55.953299999999999, -3.1882999999999999, "Edimburgo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 71, 12, null, null, false, 43.653199999999998, -79.383200000000002, "Toronto" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 72, 12, null, null, false, 45.5017, -73.567300000000003, "Montreal" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 73, 12, null, null, false, 49.282699999999998, -123.1207, "Vancouver" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 74, 12, null, null, false, 51.044699999999999, -114.0719, "Calgary" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 75, 13, null, null, false, 34.6937, 135.50229999999999, "Osaka" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 76, 13, null, null, false, 35.011600000000001, 135.7681, "Kyoto" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 77, 13, null, null, false, 34.385300000000001, 132.45529999999999, "Hiroshima" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 78, 13, null, null, false, 35.1815, 136.9066, "Nagoya" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 79, 15, null, null, false, -33.8688, 151.20930000000001, "Sydney" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 80, 15, null, null, false, -37.813600000000001, 144.9631, "Melbourne" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 81, 15, null, null, false, -27.469799999999999, 153.02510000000001, "Brisbane" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 82, 15, null, null, false, -31.950500000000002, 115.8605, "Perth" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 83, 17, null, null, false, -22.9068, -43.172899999999998, "Rio de Janeiro" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 84, 17, null, null, false, -23.5505, -46.633299999999998, "São Paulo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 85, 17, null, null, false, -12.971399999999999, -38.501399999999997, "Salvador" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 86, 1, null, null, false, 45.438400000000001, 10.9916, "Verona" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 87, 1, null, null, false, 44.4056, 8.9463000000000008, "Genova" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 88, 35, null, null, false, 41.008200000000002, 28.978400000000001, "Istanbul" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 89, 35, null, null, false, 36.896900000000002, 30.7133, "Antalya" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 90, 35, null, null, false, 38.423699999999997, 27.142800000000001, "Izmir" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 91, 31, null, null, false, 7.9519000000000002, 98.338099999999997, "Phuket" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 92, 31, null, null, false, 18.7883, 98.985299999999995, "Chiang Mai" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 93, 31, null, null, false, 12.9236, 100.8824, "Pattaya" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_PlannedTripId",
                table: "ChecklistItems",
                column: "PlannedTripId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId",
                table: "Cities",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_DreamDestinations_ApplicationUserId",
                table: "DreamDestinations",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_TravelJournalId",
                table: "Photos",
                column: "TravelJournalId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelJournals_CountryId",
                table: "TravelJournals",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelJournals_UserId",
                table: "TravelJournals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitedCountries_CountryId",
                table: "VisitedCountries",
                column: "CountryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "DreamDestinations");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "VisitedCountries");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "PlannedTrips");

            migrationBuilder.DropTable(
                name: "TravelJournals");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
