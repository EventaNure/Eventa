using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodeUsingDateTimeForOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "QrCodeUsingDateTime",
                table: "Orders",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeUsingDateTime",
                table: "Orders");
        }
    }
}
