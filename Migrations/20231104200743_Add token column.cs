using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShortenUrl.Migrations
{
    /// <inheritdoc />
    public partial class Addtokencolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Urls",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "Urls");
        }
    }
}
