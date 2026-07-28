using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handwerker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallationWizard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AvAgreementAccepted",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "UserSettings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "UserSettings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsInstallationCompleted",
                table: "UserSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "UserSettings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "UserSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralSource",
                table: "UserSettings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Salutation",
                table: "UserSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "UserSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegister",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegisterCourt",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "VatExemption",
                table: "Companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvAgreementAccepted",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "IsInstallationCompleted",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ProfileImagePath",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ReferralSource",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Salutation",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "CommercialRegister",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RegisterCourt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "VatExemption",
                table: "Companies");
        }
    }
}
