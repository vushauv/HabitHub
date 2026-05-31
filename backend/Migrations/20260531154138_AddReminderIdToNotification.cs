using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderIdToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReminderId",
                table: "Notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReminderId_CreatedAt",
                table: "Notifications",
                columns: new[] { "ReminderId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Reminders_ReminderId",
                table: "Notifications",
                column: "ReminderId",
                principalTable: "Reminders",
                principalColumn: "ReminderId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Reminders_ReminderId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ReminderId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReminderId",
                table: "Notifications");
        }
    }
}
