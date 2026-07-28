using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Handwerker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticlesAndWarehouses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArticleNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleWarehouses",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    StockQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    MinStockLevel = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxStockLevel = table.Column<decimal>(type: "numeric", nullable: true),
                    StorageLocation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleWarehouses", x => new { x.ArticleId, x.WarehouseId });
                    table.ForeignKey(
                        name: "FK_ArticleWarehouses_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleWarehouses_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArticleId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    StockBefore = table.Column<decimal>(type: "numeric", nullable: false),
                    StockAfter = table.Column<decimal>(type: "numeric", nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ArticleNumber",
                table: "Articles",
                column: "ArticleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_IsActive",
                table: "Articles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Name",
                table: "Articles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleWarehouses_StockQuantity",
                table: "ArticleWarehouses",
                column: "StockQuantity");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleWarehouses_WarehouseId",
                table: "ArticleWarehouses",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_ArticleId",
                table: "InventoryMovements",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_CreatedAt",
                table: "InventoryMovements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_ReferenceType_ReferenceId",
                table: "InventoryMovements",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_Type",
                table: "InventoryMovements",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_WarehouseId",
                table: "InventoryMovements",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_IsActive",
                table: "Warehouses",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleWarehouses");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Warehouses");
        }
    }
}
