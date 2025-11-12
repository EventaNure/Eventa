using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEventDateTimeFromTicketInCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketsInCart_EventDateTimes_EventDateTimeId",
                table: "TicketsInCart");

            migrationBuilder.DropIndex(
                name: "IX_TicketsInCart_EventDateTimeId",
                table: "TicketsInCart");

            migrationBuilder.DropColumn(
                name: "EventDateTimeId",
                table: "TicketsInCart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventDateTimeId",
                table: "TicketsInCart",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketsInCart_EventDateTimeId",
                table: "TicketsInCart",
                column: "EventDateTimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketsInCart_EventDateTimes_EventDateTimeId",
                table: "TicketsInCart",
                column: "EventDateTimeId",
                principalTable: "EventDateTimes",
                principalColumn: "Id");
        }
    }
}
