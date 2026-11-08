using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLogistiXX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatusCodeFromIdempotentRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "IdempotentRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "IdempotentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
