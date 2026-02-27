using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGiftImageUrltoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Gifts",
                newName: "GiftImagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GiftImagePath",
                table: "Gifts",
                newName: "ImageUrl");
        }
    }
}
