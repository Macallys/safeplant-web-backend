using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeviceEdgeManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DeviceLastSeen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_seen_at",
                schema: "public",
                table: "device_credentials",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_seen_at",
                schema: "public",
                table: "device_credentials");
        }
    }
}
