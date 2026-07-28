using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handwerker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersAndRelatedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CustomerNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RecipientId = table.Column<int>(type: "integer", nullable: false),
                    ProviderId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalTaxAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    InternalNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    InvoiceId = table.Column<int>(type: "integer", nullable: true),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Recipients_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Recipients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ArticleId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    IsReserved = table.Column<bool>(type: "boolean", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderMaterials_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderMaterials_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderMaterials_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderOffers",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    OfferId = table.Column<int>(type: "integer", nullable: false),
                    PortionPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderOffers", x => new { x.OrderId, x.OfferId });
                    table.ForeignKey(
                        name: "FK_OrderOffers_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderOffers_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkTimeEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    BreakDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsBillable = table.Column<bool>(type: "boolean", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric", nullable: true),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ApprovedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkTimeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkTimeEntries_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_OrderId",
                table: "Products",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterials_ArticleId",
                table: "OrderMaterials",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterials_OrderId",
                table: "OrderMaterials",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterials_WarehouseId",
                table: "OrderMaterials",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderOffers_OfferId",
                table: "OrderOffers",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerNumber",
                table: "Orders",
                column: "CustomerNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_InvoiceId",
                table: "Orders",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Priority",
                table: "Orders",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProviderId",
                table: "Orders",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RecipientId",
                table: "Orders",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTimeEntries_Date",
                table: "WorkTimeEntries",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTimeEntries_OrderId",
                table: "WorkTimeEntries",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkTimeEntries_UserId",
                table: "WorkTimeEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Orders_OrderId",
                table: "Products",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Orders_OrderId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "OrderMaterials");

            migrationBuilder.DropTable(
                name: "OrderOffers");

            migrationBuilder.DropTable(
                name: "WorkTimeEntries");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Products_OrderId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Products");
        }
    }
}
