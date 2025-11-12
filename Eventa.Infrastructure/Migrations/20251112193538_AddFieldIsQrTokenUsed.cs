using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldIsQrTokenUsed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQrTokenUsed",
                table: "Orders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQrTokenUsed",
                table: "Orders");
        }
    }
}
