using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WanderGlobe.Migrations
{
    public partial class AddMajorCities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 26, 1, null, null, false, 45.464199999999998, 9.1899999999999995, "Milano" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 27, 1, null, null, false, 40.851799999999997, 14.2681, "Napoli" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 28, 1, null, null, false, 43.769599999999997, 11.255800000000001, "Firenze" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 29, 1, null, null, false, 45.440800000000003, 12.3155, "Venezia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 30, 2, null, null, false, 43.296500000000002, 5.3697999999999997, "Marsiglia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 31, 2, null, null, false, 45.764000000000003, 4.8357000000000001, "Lione" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 32, 2, null, null, false, 43.7102, 7.2619999999999996, "Nizza" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 33, 2, null, null, false, 44.837800000000001, -0.57920000000000005, "Bordeaux" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 34, 3, null, null, false, 40.712800000000001, -74.006, "New York" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 35, 3, null, null, false, 34.052199999999999, -118.2437, "Los Angeles" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 36, 3, null, null, false, 41.878100000000003, -87.629800000000003, "Chicago" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 37, 3, null, null, false, 37.774900000000002, -122.4194, "San Francisco" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 38, 4, null, null, false, 48.135100000000001, 11.582000000000001, "Monaco" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 39, 4, null, null, false, 53.551099999999998, 9.9937000000000005, "Amburgo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 40, 4, null, null, false, 50.110900000000001, 8.6821000000000002, "Francoforte" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 41, 4, null, null, false, 50.9375, 6.9603000000000002, "Colonia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 42, 5, null, null, false, 41.385100000000001, 2.1734, "Barcellona" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 43, 5, null, null, false, 39.469900000000003, -0.37630000000000002, "Valencia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 44, 5, null, null, false, 37.389099999999999, -5.9844999999999997, "Siviglia" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 45, 5, null, null, false, 43.262999999999998, -2.9350000000000001, "Bilbao" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 46, 6, null, null, false, 41.157899999999998, -8.6290999999999993, "Porto" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 47, 6, null, null, false, 37.019300000000001, -7.9303999999999997, "Faro" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 48, 6, null, null, false, 40.203299999999999, -8.4102999999999994, "Coimbra" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 49, 6, null, null, false, 41.545400000000001, -8.4265000000000008, "Braga" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 50, 7, null, null, false, 47.376899999999999, 8.5417000000000005, "Zurigo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 51, 7, null, null, false, 46.2044, 6.1432000000000002, "Ginevra" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 52, 7, null, null, false, 47.559600000000003, 7.5885999999999996, "Basilea" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 53, 7, null, null, false, 46.5197, 6.6322999999999999, "Losanna" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 54, 11, null, null, false, 53.480800000000002, -2.2425999999999999, "Manchester" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 55, 11, null, null, false, 52.486199999999997, -1.8904000000000001, "Birmingham" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 56, 11, null, null, false, 53.4084, -2.9916, "Liverpool" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 57, 11, null, null, false, 55.953299999999999, -3.1882999999999999, "Edimburgo" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 58, 13, null, null, false, 34.6937, 135.50229999999999, "Osaka" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 59, 13, null, null, false, 35.011600000000001, 135.7681, "Kyoto" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 60, 13, null, null, false, 34.385300000000001, 132.45529999999999, "Hiroshima" });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CountryId", "Description", "ImageUrl", "IsCapital", "Latitude", "Longitude", "Name" },
                values: new object[] { 61, 13, null, null, false, 43.061799999999998, 141.3545, "Sapporo" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 60);

            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 61);
        }
    }
}
