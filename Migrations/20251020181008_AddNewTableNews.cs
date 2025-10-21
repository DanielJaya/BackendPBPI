using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendPBPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTableNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsHDR",
                columns: table => new
                {
                    NewsID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NewsSubTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsHDR", x => x.NewsID);
                    table.ForeignKey(
                        name: "FK_NewsHDR_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NewsDTL",
                columns: table => new
                {
                    NewsDTLID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsHDRID = table.Column<int>(type: "int", nullable: false),
                    NewsContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewsUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsDTL", x => x.NewsDTLID);
                    table.ForeignKey(
                        name: "FK_NewsDTL_NewsHDR_NewsHDRID",
                        column: x => x.NewsHDRID,
                        principalTable: "NewsHDR",
                        principalColumn: "NewsID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsDTL_NewsHDRID",
                table: "NewsDTL",
                column: "NewsHDRID");

            migrationBuilder.CreateIndex(
                name: "IX_NewsHDR_UserID",
                table: "NewsHDR",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsDTL");

            migrationBuilder.DropTable(
                name: "NewsHDR");
        }
    }
}
