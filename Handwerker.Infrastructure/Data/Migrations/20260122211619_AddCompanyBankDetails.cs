using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handwerker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyBankDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Bic",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Bic",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Companies");
        }
    }
}
