using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantMonitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AreaThresholds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "area_thresholds",
                schema: "public",
                columns: table => new
                {
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    co2_limit_ppm = table.Column<int>(type: "integer", nullable: false),
                    noise_limit_db = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_area_thresholds", x => x.area_id);
                    table.ForeignKey(
                        name: "FK_area_thresholds_industrial_areas_area_id",
                        column: x => x.area_id,
                        principalSchema: "public",
                        principalTable: "industrial_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "area_thresholds",
                schema: "public");
        }
    }
}
