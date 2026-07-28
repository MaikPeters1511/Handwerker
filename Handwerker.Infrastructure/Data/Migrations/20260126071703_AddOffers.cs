using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handwerker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfferId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OfferNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OfferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IntroText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OutroText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsReceived = table.Column<bool>(type: "boolean", nullable: false),
                    ConvertedToOrderId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offers_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Offers_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_OfferId",
                table: "Products",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_ProviderId",
                table: "Offers",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_RecipientId",
                table: "Offers",
                column: "RecipientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Offers_OfferId",
                table: "Products",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Offers_OfferId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Products_OfferId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "Products");
        }
    }
}
