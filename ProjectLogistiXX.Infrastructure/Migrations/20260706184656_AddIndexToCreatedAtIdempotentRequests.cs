using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLogistiXX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToCreatedAtIdempotentRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IdempotentRequests_CreatedAt",
                table: "IdempotentRequests",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdempotentRequests_CreatedAt",
                table: "IdempotentRequests");
        }
    }
}
