using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handwerker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Iban = table.Column<string>(type: "text", nullable: false),
                    Bic = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerNumber = table.Column<string>(type: "text", nullable: false),
                    Salutation = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ContactPerson = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    AddressLine2 = table.Column<string>(type: "text", nullable: false),
                    ZipCode = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Company = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    ZipCode = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: false),
                    TaxId = table.Column<string>(type: "text", nullable: false),
                    TaxNumber = table.Column<string>(type: "text", nullable: false),
                    CommercialRegister = table.Column<string>(type: "text", nullable: false),
                    RegisterCourt = table.Column<string>(type: "text", nullable: false),
                    BankDetailsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_BankAccounts_BankDetailsId",
                        column: x => x.BankDetailsId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServicePeriod = table.Column<string>(type: "text", nullable: false),
                    CustomerNumber = table.Column<string>(type: "text", nullable: false),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentTerms = table.Column<string>(type: "text", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    IntroText = table.Column<string>(type: "text", nullable: false),
                    OutroText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArticleNumber = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric", nullable: false),
                    InvoiceId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ProviderId",
                table: "Invoices",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_RecipientId",
                table: "Invoices",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_InvoiceId",
                table: "Products",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_BankDetailsId",
                table: "Providers",
                column: "BankDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.DropTable(
                name: "Recipients");

            migrationBuilder.DropTable(
                name: "BankAccounts");
        }
    }
}
