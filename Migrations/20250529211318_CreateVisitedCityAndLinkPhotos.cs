using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WanderGlobe.Migrations
{
    public partial class CreateVisitedCityAndLinkPhotos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Photos");

            migrationBuilder.AddColumn<int>(
                name: "TravelJournalId",
                table: "Photos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisitedCityId",
                table: "Photos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_VisitedCityId",
                table: "Photos",
                column: "VisitedCityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_VisitedCities_VisitedCityId",
                table: "Photos",
                column: "VisitedCityId",
                principalTable: "VisitedCities",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_VisitedCities_VisitedCityId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_VisitedCityId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "TravelJournalId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "VisitedCityId",
                table: "Photos");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Photos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
