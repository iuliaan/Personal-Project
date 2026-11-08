using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLogistiXX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToIdempotentRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "IdempotentRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests",
                columns: new[] { "Id", "UserId" })
                .Annotation("SqlServer:Clustered", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "IdempotentRequests");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdempotentRequests",
                table: "IdempotentRequests",
                column: "Id")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
