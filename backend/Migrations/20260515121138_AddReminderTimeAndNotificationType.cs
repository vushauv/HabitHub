using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderTimeAndNotificationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_UserType_Status",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderTime",
                table: "Habits",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_UserType_Status_Type",
                table: "Notifications",
                columns: new[] { "UserId", "UserType", "Status", "Type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_UserType_Status_Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReminderTime",
                table: "Habits");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_UserType_Status",
                table: "Notifications",
                columns: new[] { "UserId", "UserType", "Status" });
        }
    }
}
