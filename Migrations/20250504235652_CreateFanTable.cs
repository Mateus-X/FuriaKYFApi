using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FuriaKYFApi.Migrations
{
    /// <inheritdoc />
    public partial class CreateFanTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Interests = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Events = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AboutYou = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Document = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RedditAccessToken = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fans", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fans_DocumentNumber",
                table: "Fans",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fans_Email",
                table: "Fans",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fans_RedditAccessToken",
                table: "Fans",
                column: "RedditAccessToken",
                unique: true,
                filter: "[RedditAccessToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fans");
        }
    }
}
