using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendPBPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTableEventsAndTableRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventsHDR",
                columns: table => new
                {
                    EventID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventPic = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    EventPicFileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventPicContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventsHDR", x => x.EventID);
                    table.ForeignKey(
                        name: "FK_EventsHDR_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankingHDR",
                columns: table => new
                {
                    RankingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlayerRegions = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlayerPoints = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingHDR", x => x.RankingID);
                    table.ForeignKey(
                        name: "FK_RankingHDR_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "EventsDTL",
                columns: table => new
                {
                    EventDTLID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventHDRID = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    LocationURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TimelineEventAndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EventLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegistrationFee = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventsDTL", x => x.EventDTLID);
                    table.ForeignKey(
                        name: "FK_EventsDTL_EventsHDR_EventHDRID",
                        column: x => x.EventHDRID,
                        principalTable: "EventsHDR",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventsFTR",
                columns: table => new
                {
                    EventFTRID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventHDRID = table.Column<int>(type: "int", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EventURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventsFTR", x => x.EventFTRID);
                    table.ForeignKey(
                        name: "FK_EventsFTR_EventsHDR_EventHDRID",
                        column: x => x.EventHDRID,
                        principalTable: "EventsHDR",
                        principalColumn: "EventID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankingDTL",
                columns: table => new
                {
                    RankingDTLID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RankingHDRID = table.Column<int>(type: "int", nullable: false),
                    PlayerPic = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PlayerGender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PlayerNationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingDTL", x => x.RankingDTLID);
                    table.ForeignKey(
                        name: "FK_RankingDTL_RankingHDR_RankingHDRID",
                        column: x => x.RankingHDRID,
                        principalTable: "RankingHDR",
                        principalColumn: "RankingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RankingFTR",
                columns: table => new
                {
                    RankingFTRID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RankingHDRID = table.Column<int>(type: "int", nullable: false),
                    MatchTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MatchLevel = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    MatchResult = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatchPoints = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingFTR", x => x.RankingFTRID);
                    table.ForeignKey(
                        name: "FK_RankingFTR_RankingHDR_RankingHDRID",
                        column: x => x.RankingHDRID,
                        principalTable: "RankingHDR",
                        principalColumn: "RankingID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventsDTL_EventHDRID",
                table: "EventsDTL",
                column: "EventHDRID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventsFTR_EventHDRID",
                table: "EventsFTR",
                column: "EventHDRID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventsHDR_UserID",
                table: "EventsHDR",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RankingDTL_RankingHDRID",
                table: "RankingDTL",
                column: "RankingHDRID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankingFTR_MatchDate",
                table: "RankingFTR",
                column: "MatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_RankingFTR_RankingHDRID",
                table: "RankingFTR",
                column: "RankingHDRID");

            migrationBuilder.CreateIndex(
                name: "IX_RankingHDR_DeletedAt",
                table: "RankingHDR",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RankingHDR_PlayerName",
                table: "RankingHDR",
                column: "PlayerName");

            migrationBuilder.CreateIndex(
                name: "IX_RankingHDR_PlayerPoints",
                table: "RankingHDR",
                column: "PlayerPoints");

            migrationBuilder.CreateIndex(
                name: "IX_RankingHDR_UserID",
                table: "RankingHDR",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventsDTL");

            migrationBuilder.DropTable(
                name: "EventsFTR");

            migrationBuilder.DropTable(
                name: "RankingDTL");

            migrationBuilder.DropTable(
                name: "RankingFTR");

            migrationBuilder.DropTable(
                name: "EventsHDR");

            migrationBuilder.DropTable(
                name: "RankingHDR");
        }
    }
}
