﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cart.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCartItemOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cart_items_shopping_carts_cart_id",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "ice_level",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "size",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "sugar_level",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.DropColumn(
                name: "temperature",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.CreateTable(
                name: "cart_item_parameters",
                schema: "cart",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cart_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_item_parameters", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_item_parameters_cart_items_cart_item_id",
                        column: x => x.cart_item_id,
                        principalSchema: "cart",
                        principalTable: "cart_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cart_item_options",
                schema: "cart",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    parameter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    extra_price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_item_options", x => x.id);
                    table.ForeignKey(
                        name: "fk_cart_item_options_cart_item_parameters_parameter_id",
                        column: x => x.parameter_id,
                        principalSchema: "cart",
                        principalTable: "cart_item_parameters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Quantity_GreaterThanZero",
                schema: "cart",
                table: "cart_items",
                sql: "quantity > 0");

            migrationBuilder.CreateIndex(
                name: "ix_cart_item_options_parameter_id",
                schema: "cart",
                table: "cart_item_options",
                column: "parameter_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cart_item_parameters_cart_item_id",
                schema: "cart",
                table: "cart_item_parameters",
                column: "cart_item_id");

            migrationBuilder.AddForeignKey(
                name: "fk_cart_items_shopping_carts_cart_id",
                schema: "cart",
                table: "cart_items",
                column: "cart_id",
                principalSchema: "cart",
                principalTable: "shopping_carts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_cart_items_shopping_carts_cart_id",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.DropTable(
                name: "cart_item_options",
                schema: "cart");

            migrationBuilder.DropTable(
                name: "cart_item_parameters",
                schema: "cart");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Quantity_GreaterThanZero",
                schema: "cart",
                table: "cart_items");

            migrationBuilder.AddColumn<string>(
                name: "ice_level",
                schema: "cart",
                table: "cart_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "size",
                schema: "cart",
                table: "cart_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sugar_level",
                schema: "cart",
                table: "cart_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "temperature",
                schema: "cart",
                table: "cart_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "fk_cart_items_shopping_carts_cart_id",
                schema: "cart",
                table: "cart_items",
                column: "cart_id",
                principalSchema: "cart",
                principalTable: "shopping_carts",
                principalColumn: "id");
        }
    }
}
