using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class CalendarBookingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_equipment_users_assigned_staff_id",
                table: "equipment");

            migrationBuilder.DropIndex(
                name: "IX_bookings_slot_id",
                table: "bookings");

            migrationBuilder.AlterColumn<string>(
                name: "assigned_staff_id",
                table: "equipment",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "end_time",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "start_time",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_bookings_slot_id",
                table: "bookings",
                column: "slot_id");

            migrationBuilder.AddForeignKey(
                name: "FK_equipment_users_assigned_staff_id",
                table: "equipment",
                column: "assigned_staff_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_equipment_users_assigned_staff_id",
                table: "equipment");

            migrationBuilder.DropIndex(
                name: "IX_bookings_slot_id",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "bookings");

            migrationBuilder.AlterColumn<string>(
                name: "assigned_staff_id",
                table: "equipment",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_slot_id",
                table: "bookings",
                column: "slot_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_equipment_users_assigned_staff_id",
                table: "equipment",
                column: "assigned_staff_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
