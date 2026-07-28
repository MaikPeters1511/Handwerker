using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handwerker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTestEmailFieldsToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TestEmailBody",
                table: "UserSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestEmailSubject",
                table: "UserSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestEmailBody",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "TestEmailSubject",
                table: "UserSettings");
        }
    }
}
