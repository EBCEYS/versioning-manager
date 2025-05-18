using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace versioning_manager_api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "devices");

            migrationBuilder.EnsureSchema(
                name: "projects");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    roles = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    password = table.Column<string>(type: "character varying(132)", maxLength: 132, nullable: false),
                    salt = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    creation_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_update_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles_role",
                        column: x => x.role,
                        principalSchema: "users",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "device",
                schema: "devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    salt = table.Column<string>(type: "text", nullable: false),
                    key_expires_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    creator_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_users_creator_id",
                        column: x => x.creator_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    creator_user_id = table.Column<int>(type: "integer", nullable: false),
                    available_sources = table.Column<string[]>(type: "text[]", nullable: false),
                    creation_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                    table.ForeignKey(
                        name: "FK_projects_users_creator_user_id",
                        column: x => x.creator_user_id,
                        principalSchema: "users",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_entries",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    last_update_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_actual = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_entries_projects_project",
                        column: x => x.project,
                        principalSchema: "projects",
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "images",
                schema: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    service_name = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    image_tag = table.Column<string>(type: "text", nullable: false),
                    docker_compose_file = table.Column<string>(type: "text", nullable: false),
                    project_entry = table.Column<int>(type: "integer", nullable: false),
                    creator_device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    creation_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_images_device_creator_device_id",
                        column: x => x.creator_device_id,
                        principalSchema: "devices",
                        principalTable: "device",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_images_project_entries_project_entry",
                        column: x => x.project_entry,
                        principalSchema: "projects",
                        principalTable: "project_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_device_creator_id",
                schema: "devices",
                table: "device",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_source",
                schema: "devices",
                table: "device",
                column: "source");

            migrationBuilder.CreateIndex(
                name: "IX_images_creator_device_id",
                schema: "projects",
                table: "images",
                column: "creator_device_id");

            migrationBuilder.CreateIndex(
                name: "IX_images_project_entry",
                schema: "projects",
                table: "images",
                column: "project_entry");

            migrationBuilder.CreateIndex(
                name: "IX_project_entries_project",
                schema: "projects",
                table: "project_entries",
                column: "project");

            migrationBuilder.CreateIndex(
                name: "IX_projects_creator_user_id",
                schema: "projects",
                table: "projects",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_name",
                schema: "projects",
                table: "projects",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                schema: "users",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role",
                schema: "users",
                table: "users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                schema: "users",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "images",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "device",
                schema: "devices");

            migrationBuilder.DropTable(
                name: "project_entries",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "projects");

            migrationBuilder.DropTable(
                name: "users",
                schema: "users");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "users");
        }
    }
}
