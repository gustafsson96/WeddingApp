using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeddingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeHeaderImageUrltoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HeaderImageUrl",
                table: "Weddings",
                newName: "HeaderImagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HeaderImagePath",
                table: "Weddings",
                newName: "HeaderImageUrl");
        }
    }
}
