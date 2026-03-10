using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCocktailAndIngredientTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$TKNcswsMVJd4iVlDKqv0SOJx5uEmoOdilcmmVEmcg38tJAQPHhOCm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$9bR0BASiWAth/BLLwVU/pOUJ.ZVrF7W31MGgpFcSGl7Nj8Il/vTo6");
        }
    }
}
