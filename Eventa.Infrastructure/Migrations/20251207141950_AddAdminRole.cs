using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "3", null, "Admin", "ADMIN" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "EventDateTimeId", "GoogleId", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "Organization", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TicketsExpireAt", "TwoFactorEnabled", "UserName", "VerificationCode" },
                values: new object[] { "1ed1ffc6-ab52-4bca-99e4-b9d8ee6b3816", 0, "b1f6f6a5-6dcb-4f3c-8f2d-5e3e5c6e4f7a", "titarenkonik3@gmail.com", true, null, "", false, null, "Mykyta", "TITARENKONIK3@GMAIL.COM", "TITARENKONIK3@GMAIL.COM", null, "AQAAAAIAAYagAAAAELoXW63IxkcpEFdKf/Sx+nPK3jiaqOUQVlvd42AXSaee4gif7itFgKwVmFUn2wnRhA==", null, false, "a1b2c3d4e5f6g7h8i9j0", null, false, "titarenkonik3@gmail.com", "" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "3", "1ed1ffc6-ab52-4bca-99e4-b9d8ee6b3816" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "1ed1ffc6-ab52-4bca-99e4-b9d8ee6b3816" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1ed1ffc6-ab52-4bca-99e4-b9d8ee6b3816");
        }
    }
}
