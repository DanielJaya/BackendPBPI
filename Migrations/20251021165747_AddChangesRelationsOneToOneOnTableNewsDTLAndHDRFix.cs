using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendPBPI.Migrations
{
    /// <inheritdoc />
    public partial class AddChangesRelationsOneToOneOnTableNewsDTLAndHDRFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsHDR_Users_UserID",
                table: "NewsHDR");

            migrationBuilder.DropIndex(
                name: "IX_NewsDTL_NewsHDRID",
                table: "NewsDTL");

            migrationBuilder.CreateIndex(
                name: "IX_NewsDTL_NewsHDRID",
                table: "NewsDTL",
                column: "NewsHDRID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsHDR_Users_UserID",
                table: "NewsHDR",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsHDR_Users_UserID",
                table: "NewsHDR");

            migrationBuilder.DropIndex(
                name: "IX_NewsDTL_NewsHDRID",
                table: "NewsDTL");

            migrationBuilder.CreateIndex(
                name: "IX_NewsDTL_NewsHDRID",
                table: "NewsDTL",
                column: "NewsHDRID");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsHDR_Users_UserID",
                table: "NewsHDR",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
