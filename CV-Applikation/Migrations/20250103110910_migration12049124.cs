using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_Applikation.Migrations
{
    /// <inheritdoc />
    public partial class migration12049124 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "CVs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "CVs");
        }
    }
}
