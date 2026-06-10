using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_UserType",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_UserType_Status_Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_Status_Type",
                table: "Notifications",
                columns: new[] { "UserId", "Status", "Type" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_Status_Type",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "Notifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_UserType",
                table: "Notifications",
                columns: new[] { "UserId", "UserType" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_UserType_Status_Type",
                table: "Notifications",
                columns: new[] { "UserId", "UserType", "Status", "Type" });
        }
    }
}
