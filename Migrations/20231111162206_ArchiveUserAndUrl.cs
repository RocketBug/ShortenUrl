using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortenUrl.Migrations
{
    /// <inheritdoc />
    public partial class ArchiveUserAndUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                table: "Urls",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Archived",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Archived",
                table: "Urls");
        }
    }
}
