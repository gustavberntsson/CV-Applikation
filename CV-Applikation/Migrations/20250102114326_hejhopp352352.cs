using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_Applikation.Migrations
{
    /// <inheritdoc />
    public partial class hejhopp352352 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adress",
                table: "ContactInformation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adress",
                table: "ContactInformation");
        }
    }
}
