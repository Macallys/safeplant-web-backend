using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantMonitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Readings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carbon_dioxide_readings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ppm = table.Column<int>(type: "integer", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carbon_dioxide_readings", x => x.id);
                    table.ForeignKey(
                        name: "FK_carbon_dioxide_readings_industrial_areas_area_id",
                        column: x => x.area_id,
                        principalSchema: "public",
                        principalTable: "industrial_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "noise_readings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    db = table.Column<int>(type: "integer", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_noise_readings", x => x.id);
                    table.ForeignKey(
                        name: "FK_noise_readings_industrial_areas_area_id",
                        column: x => x.area_id,
                        principalSchema: "public",
                        principalTable: "industrial_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "presence_changes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presence_changes", x => x.id);
                    table.ForeignKey(
                        name: "FK_presence_changes_industrial_areas_area_id",
                        column: x => x.area_id,
                        principalSchema: "public",
                        principalTable: "industrial_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_carbon_dioxide_readings_area_id_recorded_at",
                schema: "public",
                table: "carbon_dioxide_readings",
                columns: new[] { "area_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_noise_readings_area_id_recorded_at",
                schema: "public",
                table: "noise_readings",
                columns: new[] { "area_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_presence_changes_area_id_recorded_at",
                schema: "public",
                table: "presence_changes",
                columns: new[] { "area_id", "recorded_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carbon_dioxide_readings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "noise_readings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "presence_changes",
                schema: "public");
        }
    }
}
