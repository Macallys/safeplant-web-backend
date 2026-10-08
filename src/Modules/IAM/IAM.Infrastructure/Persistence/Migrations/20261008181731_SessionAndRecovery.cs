using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IAM.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SessionAndRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "closed_at",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "recovery_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recovery_tokens_token_hash",
                table: "recovery_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recovery_tokens");

            migrationBuilder.DropColumn(
                name: "closed_at",
                table: "sessions");
        }
    }
}
