using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_Applikation.Migrations
{
    /// <inheritdoc />
    public partial class useridkontakt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactInformation_CVs_CVId",
                table: "ContactInformation");

            migrationBuilder.DropIndex(
                name: "IX_ContactInformation_CVId",
                table: "ContactInformation");

            migrationBuilder.DropColumn(
                name: "CVId",
                table: "ContactInformation");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ContactInformation",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInformation_UserId",
                table: "ContactInformation",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInformation_AspNetUsers_UserId",
                table: "ContactInformation",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactInformation_AspNetUsers_UserId",
                table: "ContactInformation");

            migrationBuilder.DropIndex(
                name: "IX_ContactInformation_UserId",
                table: "ContactInformation");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ContactInformation");

            migrationBuilder.AddColumn<int>(
                name: "CVId",
                table: "ContactInformation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ContactInformation_CVId",
                table: "ContactInformation",
                column: "CVId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactInformation_CVs_CVId",
                table: "ContactInformation",
                column: "CVId",
                principalTable: "CVs",
                principalColumn: "CVId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
