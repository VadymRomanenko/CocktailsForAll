using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtendedDescription",
                table: "CocktailTranslations",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$8OlL.PCF/I3wsRQ0RCK/4.ORKAfGzQAtN1B5pO/3LuBfaIBOeyNHC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtendedDescription",
                table: "CocktailTranslations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$RNKloeg9cc6b0VJKllznLOVU6KMHXa.GgD6KHzC21ijB7ziqA3ZUK");
        }
    }
}
