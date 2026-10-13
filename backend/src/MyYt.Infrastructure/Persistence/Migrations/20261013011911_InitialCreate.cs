using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyYt.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false),
                    display_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, collation: "utf8mb4_bin")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_tags_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "videos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    youtube_id = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false),
                    title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    channel_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    thumbnail_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_videos_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "video_tags",
                columns: table => new
                {
                    video_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    tag_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_tags", x => new { x.video_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_video_tags_tags",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_video_tags_videos",
                        column: x => x.video_id,
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ux_tags_user_name",
                table: "tags",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_video_tags_tag",
                table: "video_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_videos_user_created",
                table: "videos",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_videos_user_youtube",
                table: "videos",
                columns: new[] { "user_id", "youtube_id" },
                unique: true);

            // The MySQL provider drops column collations from its DDL; tag names must compare exactly
            // (binary) so the default accent/case-insensitive collation doesn't merge e.g. "café" and "cafe".
            migrationBuilder.Sql(
                "ALTER TABLE `tags` MODIFY `name` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "video_tags");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "videos");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
