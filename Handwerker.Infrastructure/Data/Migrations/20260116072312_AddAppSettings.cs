using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handwerker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Theme = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    PushNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    SmsNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    InvoicePrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NextInvoiceNumber = table.Column<int>(type: "integer", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }
    }
}
