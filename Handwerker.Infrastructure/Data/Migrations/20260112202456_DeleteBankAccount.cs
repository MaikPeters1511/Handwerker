using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handwerker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_BankAccounts_BankDetailsId",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.RenameColumn(
                name: "BankDetailsId",
                table: "Providers",
                newName: "BankId");

            migrationBuilder.RenameIndex(
                name: "IX_Providers_BankDetailsId",
                table: "Providers",
                newName: "IX_Providers_BankId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Banks_BankId",
                table: "Providers",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Banks_BankId",
                table: "Providers");

            migrationBuilder.RenameColumn(
                name: "BankId",
                table: "Providers",
                newName: "BankDetailsId");

            migrationBuilder.RenameIndex(
                name: "IX_Providers_BankId",
                table: "Providers",
                newName: "IX_Providers_BankDetailsId");

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BankName = table.Column<string>(type: "text", nullable: false),
                    Bic = table.Column<string>(type: "text", nullable: false),
                    Iban = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_BankAccounts_BankDetailsId",
                table: "Providers",
                column: "BankDetailsId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
