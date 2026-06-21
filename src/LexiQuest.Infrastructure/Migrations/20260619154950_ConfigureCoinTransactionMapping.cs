using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiQuest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCoinTransactionMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CoinTransaction_CreatedAt",
                table: "CoinTransaction",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CoinTransaction_CreatedAt",
                table: "CoinTransaction");
        }
    }
}
