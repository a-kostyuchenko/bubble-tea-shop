﻿// <auto-generated/>
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.API.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.CreateTable(
                name: "bubble_teas",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    tea_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bubble_teas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingredients",
                schema: "catalog",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingredients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bubble_tea_ingredients",
                schema: "catalog",
                columns: table => new
                {
                    bubble_tea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ingredients_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bubble_tea_ingredients", x => new { x.bubble_tea_id, x.ingredients_id });
                    table.ForeignKey(
                        name: "fk_bubble_tea_ingredients_bubble_teas_bubble_tea_id",
                        column: x => x.bubble_tea_id,
                        principalSchema: "catalog",
                        principalTable: "bubble_teas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bubble_tea_ingredients_ingredients_ingredients_id",
                        column: x => x.ingredients_id,
                        principalSchema: "catalog",
                        principalTable: "ingredients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bubble_tea_ingredients_ingredients_id",
                schema: "catalog",
                table: "bubble_tea_ingredients",
                column: "ingredients_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bubble_tea_ingredients",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "bubble_teas",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "ingredients",
                schema: "catalog");
        }
    }
}
