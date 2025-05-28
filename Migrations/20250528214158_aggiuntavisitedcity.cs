using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WanderGlobe.Migrations
{
    public partial class aggiuntavisitedcity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DreamCountries_AspNetUsers_ApplicationUserId",
                table: "DreamCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_DreamDestinations_AspNetUsers_ApplicationUserId",
                table: "DreamDestinations");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_TravelJournals_TravelJournalUserId_TravelJournalCountryId_TravelJournalVisitDate",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_DreamDestinations_ApplicationUserId",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "CountryName",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "DreamDestinations");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "DreamDestinations",
                newName: "AddedDate");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "DreamDestinations",
                newName: "TargetDate");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "TravelJournals",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TravelJournals",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TravelJournals",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TravelJournals",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationName",
                table: "PlannedTrips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PlannedTrips",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "TravelJournalVisitDate",
                table: "Photos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TravelJournalUserId",
                table: "Photos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "TravelJournalCountryId",
                table: "Photos",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "Photos",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "DreamDestinations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "DreamDestinations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "DreamDestinations",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationName",
                table: "DreamDestinations",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAchieved",
                table: "DreamDestinations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "DreamCountries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "DreamCountries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CriteriaType",
                table: "Badges",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequiredCount",
                table: "Badges",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VisitedCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CityId = table.Column<int>(type: "INTEGER", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitedCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitedCities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitedCities_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 5, 41.385100000000001, 2.1734, "Barcellona" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 43.769599999999997, 11.255800000000001, "Firenze" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 45.440800000000003, 12.3155, "Venezia" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 44.494900000000001, 11.342599999999999, "Bologna" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 45.070300000000003, 7.6868999999999996, "Torino" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 1, 38.115699999999997, 13.361499999999999, "Palermo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 43.296500000000002, 5.3697999999999997, "Marsiglia" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 45.764000000000003, 4.8357000000000001, "Lione" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 43.7102, 7.2619999999999996, "Nizza" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 44.837800000000001, -0.57920000000000005, "Bordeaux" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 43.604700000000001, 1.4441999999999999, "Tolosa" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 2, 48.573399999999999, 7.7521000000000004, "Strasburgo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 40.712800000000001, -74.006, "New York" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 34.052199999999999, -118.2437, "Los Angeles" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 41.878100000000003, -87.629800000000003, "Chicago" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 25.761700000000001, -80.191800000000001, "Miami" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 37.774900000000002, -122.4194, "San Francisco" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 36.169899999999998, -115.13979999999999, "Las Vegas" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 3, 42.360100000000003, -71.058899999999994, "Boston" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 48.135100000000001, 11.582000000000001, "Monaco" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 53.551099999999998, 9.9937000000000005, "Amburgo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 50.110900000000001, 8.6821000000000002, "Francoforte" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 50.9375, 6.9603000000000002, "Colonia" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 61,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 4, 51.227699999999999, 6.7735000000000003, "Düsseldorf" });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UserId",
                table: "Photos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DreamDestinations_CityId",
                table: "DreamDestinations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_DreamDestinations_CountryId",
                table: "DreamDestinations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_DreamDestinations_UserId",
                table: "DreamDestinations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DreamCountries_UserId_CountryId",
                table: "DreamCountries",
                columns: new[] { "UserId", "CountryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitedCities_CityId",
                table: "VisitedCities",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitedCities_UserId_CityId_VisitDate",
                table: "VisitedCities",
                columns: new[] { "UserId", "CityId", "VisitDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_DreamCountries_AspNetUsers_ApplicationUserId",
                table: "DreamCountries",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DreamCountries_AspNetUsers_UserId",
                table: "DreamCountries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DreamDestinations_AspNetUsers_UserId",
                table: "DreamDestinations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DreamDestinations_Cities_CityId",
                table: "DreamDestinations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DreamDestinations_Countries_CountryId",
                table: "DreamDestinations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_AspNetUsers_UserId",
                table: "Photos",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_TravelJournals_TravelJournalUserId_TravelJournalCountryId_TravelJournalVisitDate",
                table: "Photos",
                columns: new[] { "TravelJournalUserId", "TravelJournalCountryId", "TravelJournalVisitDate" },
                principalTable: "TravelJournals",
                principalColumns: new[] { "UserId", "CountryId", "VisitDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DreamCountries_AspNetUsers_ApplicationUserId",
                table: "DreamCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_DreamCountries_AspNetUsers_UserId",
                table: "DreamCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_DreamDestinations_AspNetUsers_UserId",
                table: "DreamDestinations");

            migrationBuilder.DropForeignKey(
                name: "FK_DreamDestinations_Cities_CityId",
                table: "DreamDestinations");

            migrationBuilder.DropForeignKey(
                name: "FK_DreamDestinations_Countries_CountryId",
                table: "DreamDestinations");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_AspNetUsers_UserId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_TravelJournals_TravelJournalUserId_TravelJournalCountryId_TravelJournalVisitDate",
                table: "Photos");

            migrationBuilder.DropTable(
                name: "VisitedCities");

            migrationBuilder.DropIndex(
                name: "IX_Photos_UserId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_DreamDestinations_CityId",
                table: "DreamDestinations");

            migrationBuilder.DropIndex(
                name: "IX_DreamDestinations_CountryId",
                table: "DreamDestinations");

            migrationBuilder.DropIndex(
                name: "IX_DreamDestinations_UserId",
                table: "DreamDestinations");

            migrationBuilder.DropIndex(
                name: "IX_DreamCountries_UserId_CountryId",
                table: "DreamCountries");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "TravelJournals");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TravelJournals");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TravelJournals");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TravelJournals");

            migrationBuilder.DropColumn(
                name: "DestinationName",
                table: "PlannedTrips");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PlannedTrips");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "DestinationName",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "IsAchieved",
                table: "DreamDestinations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DreamCountries");

            migrationBuilder.DropColumn(
                name: "CriteriaType",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "RequiredCount",
                table: "Badges");

            migrationBuilder.RenameColumn(
                name: "TargetDate",
                table: "DreamDestinations",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "AddedDate",
                table: "DreamDestinations",
                newName: "ImageUrl");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TravelJournalVisitDate",
                table: "Photos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TravelJournalUserId",
                table: "Photos",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TravelJournalCountryId",
                table: "Photos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Caption",
                table: "Photos",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "DreamDestinations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "DreamDestinations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "DreamDestinations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                table: "DreamDestinations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "DreamDestinations",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "DreamDestinations",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "ApplicationUserId",
                table: "DreamCountries",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 1, 43.769599999999997, 11.255800000000001, "Firenze" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 45.440800000000003, 12.3155, "Venezia" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 44.494900000000001, 11.342599999999999, "Bologna" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 45.070300000000003, 7.6868999999999996, "Torino" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 38.115699999999997, 13.361499999999999, "Palermo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 2, 43.296500000000002, 5.3697999999999997, "Marsiglia" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 45.764000000000003, 4.8357000000000001, "Lione" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 43.7102, 7.2619999999999996, "Nizza" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 44.837800000000001, -0.57920000000000005, "Bordeaux" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 43.604700000000001, 1.4441999999999999, "Tolosa" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 48.573399999999999, 7.7521000000000004, "Strasburgo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 3, 40.712800000000001, -74.006, "New York" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 34.052199999999999, -118.2437, "Los Angeles" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 51,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 41.878100000000003, -87.629800000000003, "Chicago" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 52,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 25.761700000000001, -80.191800000000001, "Miami" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 37.774900000000002, -122.4194, "San Francisco" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 36.169899999999998, -115.13979999999999, "Las Vegas" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 55,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 42.360100000000003, -71.058899999999994, "Boston" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 56,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 4, 48.135100000000001, 11.582000000000001, "Monaco" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 57,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 53.551099999999998, 9.9937000000000005, "Amburgo" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 58,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 50.110900000000001, 8.6821000000000002, "Francoforte" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 59,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 50.9375, 6.9603000000000002, "Colonia" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 60,
                columns: new[] { "Latitude", "Longitude", "Name" },
                values: new object[] { 51.227699999999999, 6.7735000000000003, "Düsseldorf" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 61,
                columns: new[] { "CountryId", "Latitude", "Longitude", "Name" },
                values: new object[] { 5, 41.385100000000001, 2.1734, "Barcellona" });

            migrationBuilder.CreateIndex(
                name: "IX_DreamDestinations_ApplicationUserId",
                table: "DreamDestinations",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DreamCountries_AspNetUsers_ApplicationUserId",
                table: "DreamCountries",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DreamDestinations_AspNetUsers_ApplicationUserId",
                table: "DreamDestinations",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_TravelJournals_TravelJournalUserId_TravelJournalCountryId_TravelJournalVisitDate",
                table: "Photos",
                columns: new[] { "TravelJournalUserId", "TravelJournalCountryId", "TravelJournalVisitDate" },
                principalTable: "TravelJournals",
                principalColumns: new[] { "UserId", "CountryId", "VisitDate" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
