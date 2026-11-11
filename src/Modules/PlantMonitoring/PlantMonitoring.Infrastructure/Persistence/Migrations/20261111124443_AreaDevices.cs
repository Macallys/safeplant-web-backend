using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantMonitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AreaDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "area_devices",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_area_devices", x => x.id);
                    table.ForeignKey(
                        name: "FK_area_devices_industrial_areas_area_id",
                        column: x => x.area_id,
                        principalSchema: "public",
                        principalTable: "industrial_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_area_devices_area_id",
                schema: "public",
                table: "area_devices",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_area_devices_device_id",
                schema: "public",
                table: "area_devices",
                column: "device_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "area_devices",
                schema: "public");
        }
    }
}
