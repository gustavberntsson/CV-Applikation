using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_Applikation.Migrations
{
    /// <inheritdoc />
    public partial class contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactInformation",
                table: "ContactInformation");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ContactInformation",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ContactInformation",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactInformation",
                table: "ContactInformation",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactInformation",
                table: "ContactInformation");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContactInformation");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ContactInformation",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactInformation",
                table: "ContactInformation",
                column: "Email");
        }
    }
}
