using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CV_Applikation.Migrations
{
    /// <inheritdoc />
    public partial class viewcount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "ContactInformation",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "ContactInformation");
        }
    }
}
