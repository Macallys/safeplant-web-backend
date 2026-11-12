using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantMonitoring.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReadingTimeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_presence_changes",
                schema: "public",
                table: "presence_changes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_noise_readings",
                schema: "public",
                table: "noise_readings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_carbon_dioxide_readings",
                schema: "public",
                table: "carbon_dioxide_readings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_presence_changes",
                schema: "public",
                table: "presence_changes",
                columns: new[] { "recorded_at", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_noise_readings",
                schema: "public",
                table: "noise_readings",
                columns: new[] { "recorded_at", "id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_carbon_dioxide_readings",
                schema: "public",
                table: "carbon_dioxide_readings",
                columns: new[] { "recorded_at", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_presence_changes",
                schema: "public",
                table: "presence_changes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_noise_readings",
                schema: "public",
                table: "noise_readings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_carbon_dioxide_readings",
                schema: "public",
                table: "carbon_dioxide_readings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_presence_changes",
                schema: "public",
                table: "presence_changes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_noise_readings",
                schema: "public",
                table: "noise_readings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_carbon_dioxide_readings",
                schema: "public",
                table: "carbon_dioxide_readings",
                column: "id");
        }
    }
}
