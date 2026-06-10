using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHabitAndCreatorRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Habits_TeamCreators_CreatorId",
                table: "Habits");

            migrationBuilder.DropIndex(
                name: "IX_Habits_CreatorId",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Habits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Habits",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Habits_CreatorId",
                table: "Habits",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Habits_TeamCreators_CreatorId",
                table: "Habits",
                column: "CreatorId",
                principalTable: "TeamCreators",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
