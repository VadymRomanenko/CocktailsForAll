using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CocktailHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCountriesRenameRussiaAndAddMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "ruZZia");

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "IsoCode", "Name" },
                values: new object[,]
                {
                    { 13, "PL", "Poland" },
                    { 14, "UA", "Ukraine" },
                    { 15, "GR", "Greece" },
                    { 16, "PT", "Portugal" },
                    { 17, "AR", "Argentina" },
                    { 18, "CO", "Colombia" },
                    { 19, "JM", "Jamaica" },
                    { 20, "TH", "Thailand" },
                    { 21, "CN", "China" },
                    { 22, "IN", "India" },
                    { 23, "AU", "Australia" },
                    { 24, "CA", "Canada" },
                    { 25, "NL", "Netherlands" },
                    { 26, "BE", "Belgium" },
                    { 27, "AT", "Austria" },
                    { 28, "SE", "Sweden" },
                    { 29, "NO", "Norway" },
                    { 30, "DK", "Denmark" },
                    { 31, "FI", "Finland" },
                    { 32, "CH", "Switzerland" },
                    { 33, "CZ", "Czech Republic" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$9bR0BASiWAth/BLLwVU/pOUJ.ZVrF7W31MGgpFcSGl7Nj8Il/vTo6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 8,
                column: "Name",
                value: "Russia");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$I1K7kMFl4YfH/bgB/Fds9O8vhbLB44LQl5lb/Z4A.neo8AM0ZCn.u");
        }
    }
}
