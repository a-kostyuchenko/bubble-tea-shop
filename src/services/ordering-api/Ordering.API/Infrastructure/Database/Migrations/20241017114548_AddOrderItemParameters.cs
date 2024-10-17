﻿// <auto-generated/>
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ice_level",
                schema: "ordering",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "size",
                schema: "ordering",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "sugar_level",
                schema: "ordering",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "temperature",
                schema: "ordering",
                table: "order_items");

            migrationBuilder.CreateTable(
                name: "order_item_parameters",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    option = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    extra_price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item_parameters", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_item_parameters_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalSchema: "ordering",
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_item_parameters_order_item_id",
                schema: "ordering",
                table: "order_item_parameters",
                column: "order_item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_item_parameters",
                schema: "ordering");

            migrationBuilder.AddColumn<string>(
                name: "ice_level",
                schema: "ordering",
                table: "order_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "size",
                schema: "ordering",
                table: "order_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sugar_level",
                schema: "ordering",
                table: "order_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "temperature",
                schema: "ordering",
                table: "order_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}