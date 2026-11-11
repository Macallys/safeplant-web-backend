using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyActuation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SafetyState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "actuator_actions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actuator_actions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "actuator_states",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    run_state = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    mode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    last_changed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actuator_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "environmental_alerts",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    raised_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    withdrawn_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_environmental_alerts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exposure_states",
                schema: "public",
                columns: table => new
                {
                    area_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    highlighted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exposure_states", x => x.area_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_actuator_actions_area_id_recorded_at",
                schema: "public",
                table: "actuator_actions",
                columns: new[] { "area_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_actuator_states_area_id_type",
                schema: "public",
                table: "actuator_states",
                columns: new[] { "area_id", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_environmental_alerts_area_id_raised_at",
                schema: "public",
                table: "environmental_alerts",
                columns: new[] { "area_id", "raised_at" });

            migrationBuilder.Sql(
                """
                ALTER TABLE public.exposure_states
                ADD CONSTRAINT fk_exposure_states_industrial_areas_area_id
                FOREIGN KEY (area_id) REFERENCES public.industrial_areas (id) ON DELETE RESTRICT;

                ALTER TABLE public.actuator_states
                ADD CONSTRAINT fk_actuator_states_industrial_areas_area_id
                FOREIGN KEY (area_id) REFERENCES public.industrial_areas (id) ON DELETE RESTRICT;

                ALTER TABLE public.environmental_alerts
                ADD CONSTRAINT fk_environmental_alerts_industrial_areas_area_id
                FOREIGN KEY (area_id) REFERENCES public.industrial_areas (id) ON DELETE RESTRICT;

                ALTER TABLE public.actuator_actions
                ADD CONSTRAINT fk_actuator_actions_industrial_areas_area_id
                FOREIGN KEY (area_id) REFERENCES public.industrial_areas (id) ON DELETE RESTRICT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public.actuator_actions DROP CONSTRAINT fk_actuator_actions_industrial_areas_area_id;
                ALTER TABLE public.environmental_alerts DROP CONSTRAINT fk_environmental_alerts_industrial_areas_area_id;
                ALTER TABLE public.actuator_states DROP CONSTRAINT fk_actuator_states_industrial_areas_area_id;
                ALTER TABLE public.exposure_states DROP CONSTRAINT fk_exposure_states_industrial_areas_area_id;
                """);

            migrationBuilder.DropTable(
                name: "actuator_actions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "actuator_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "environmental_alerts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "exposure_states",
                schema: "public");
        }
    }
}
