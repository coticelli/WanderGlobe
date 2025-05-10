using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WanderGlobe.Migrations
{
    public partial class InitialMigration : Migration
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
                name: "IX_Cities_CountryId",
                table: "Cities",
                column: "CountryId");

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
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "VisitedCountries");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

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
