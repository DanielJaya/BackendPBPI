using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendPBPI.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnNewsPicAsByte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "NewsPic",
                table: "NewsHDR",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "NewsPicContentType",
                table: "NewsHDR",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NewsPicFileName",
                table: "NewsHDR",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsPic",
                table: "NewsHDR");

            migrationBuilder.DropColumn(
                name: "NewsPicContentType",
                table: "NewsHDR");

            migrationBuilder.DropColumn(
                name: "NewsPicFileName",
                table: "NewsHDR");
        }
    }
}
