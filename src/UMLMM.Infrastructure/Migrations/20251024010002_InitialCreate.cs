using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UMLMM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_20251024010002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sources",
                columns: table => new
                {
                    source_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    base_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sources", x => x.source_id);
                });

            migrationBuilder.CreateTable(
                name: "fetch_runs",
                columns: table => new
                {
                    run_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    stats = table.Column<string>(type: "jsonb", nullable: true),
                    error_text = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fetch_runs", x => x.run_id);
                    table.ForeignKey(
                        name: "FK_fetch_runs_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "models",
                columns: table => new
                {
                    model_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    nsfw_rating = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    raw = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_models", x => x.model_id);
                    table.ForeignKey(
                        name: "FK_models_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    tag_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.tag_id);
                    table.ForeignKey(
                        name: "FK_tags_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    workflow_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    external_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    graph = table.Column<string>(type: "jsonb", nullable: true),
                    nodes_count = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.workflow_id);
                    table.ForeignKey(
                        name: "FK_workflows_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "model_versions",
                columns: table => new
                {
                    model_version_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_id = table.Column<int>(type: "integer", nullable: false),
                    version_label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    checksum = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_versions", x => x.model_version_id);
                    table.ForeignKey(
                        name: "FK_model_versions_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "model_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "model_tags",
                columns: table => new
                {
                    model_id = table.Column<int>(type: "integer", nullable: false),
                    tag_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_tags", x => new { x.model_id, x.tag_id });
                    table.ForeignKey(
                        name: "FK_model_tags_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "model_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_model_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "tag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "artifacts",
                columns: table => new
                {
                    artifact_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_version_id = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    file_name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_artifacts", x => x.artifact_id);
                    table.ForeignKey(
                        name: "FK_artifacts_model_versions_model_version_id",
                        column: x => x.model_version_id,
                        principalTable: "model_versions",
                        principalColumn: "model_version_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    image_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_id = table.Column<int>(type: "integer", nullable: true),
                    model_version_id = table.Column<int>(type: "integer", nullable: true),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    rating = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.image_id);
                    table.ForeignKey(
                        name: "FK_images_model_versions_model_version_id",
                        column: x => x.model_version_id,
                        principalTable: "model_versions",
                        principalColumn: "model_version_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_images_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "model_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prompts",
                columns: table => new
                {
                    prompt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_id = table.Column<int>(type: "integer", nullable: true),
                    model_version_id = table.Column<int>(type: "integer", nullable: true),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompts", x => x.prompt_id);
                    table.ForeignKey(
                        name: "FK_prompts_model_versions_model_version_id",
                        column: x => x.model_version_id,
                        principalTable: "model_versions",
                        principalColumn: "model_version_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prompts_models_model_id",
                        column: x => x.model_id,
                        principalTable: "models",
                        principalColumn: "model_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prompts_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_model_version_id",
                table: "artifacts",
                column: "model_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_artifacts_sha256",
                table: "artifacts",
                column: "sha256");

            migrationBuilder.CreateIndex(
                name: "IX_fetch_runs_source_id",
                table: "fetch_runs",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_fetch_runs_started_at",
                table: "fetch_runs",
                column: "started_at");

            migrationBuilder.CreateIndex(
                name: "IX_fetch_runs_status",
                table: "fetch_runs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_images_model_id",
                table: "images",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_images_model_version_id",
                table: "images",
                column: "model_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_images_sha256",
                table: "images",
                column: "sha256");

            migrationBuilder.CreateIndex(
                name: "IX_model_tags_model_id",
                table: "model_tags",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_tags_tag_id",
                table: "model_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_versions_model_id",
                table: "model_versions",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_versions_model_id_version_label",
                table: "model_versions",
                columns: new[] { "model_id", "version_label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_models_created_at",
                table: "models",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_models_source_id",
                table: "models",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_models_source_id_external_id",
                table: "models",
                columns: new[] { "source_id", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_models_type",
                table: "models",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_model_id",
                table: "prompts",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_model_version_id",
                table: "prompts",
                column: "model_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_source_id",
                table: "prompts",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_sources_name",
                table: "sources",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_normalized_name",
                table: "tags",
                column: "normalized_name");

            migrationBuilder.CreateIndex(
                name: "IX_tags_normalized_name_source_id",
                table: "tags",
                columns: new[] { "normalized_name", "source_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_source_id",
                table: "tags",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_source_id",
                table: "workflows",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_source_id_external_id",
                table: "workflows",
                columns: new[] { "source_id", "external_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artifacts");

            migrationBuilder.DropTable(
                name: "fetch_runs");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "model_tags");

            migrationBuilder.DropTable(
                name: "prompts");

            migrationBuilder.DropTable(
                name: "workflows");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "model_versions");

            migrationBuilder.DropTable(
                name: "models");

            migrationBuilder.DropTable(
                name: "sources");
        }
    }
}
