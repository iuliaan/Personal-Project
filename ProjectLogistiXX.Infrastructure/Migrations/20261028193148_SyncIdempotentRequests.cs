using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLogistiXX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncIdempotentRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests");

            migrationBuilder.DropIndex(
                name: "IX_IdempotentRequests_CreatedAt",
                table: "IdempotentRequests");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "IdempotentRequests",
                newName: "CreatedAtUtc");

            migrationBuilder.AlterColumn<string>(
                name: "Response",
                table: "IdempotentRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IdempotentRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "IdempotentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LockToken",
                table: "IdempotentRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "IdempotentRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_IdempotentRequests_CreatedAtUtc",
                table: "IdempotentRequests",
                column: "CreatedAtUtc")
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests");

            migrationBuilder.DropIndex(
                name: "IX_IdempotentRequests_CreatedAtUtc",
                table: "IdempotentRequests");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "IdempotentRequests");

            migrationBuilder.DropColumn(
                name: "LockToken",
                table: "IdempotentRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "IdempotentRequests");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "IdempotentRequests",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Response",
                table: "IdempotentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "IdempotentRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotentRequests_CreatedAt",
                table: "IdempotentRequests",
                column: "CreatedAt");
        }
    }
}
