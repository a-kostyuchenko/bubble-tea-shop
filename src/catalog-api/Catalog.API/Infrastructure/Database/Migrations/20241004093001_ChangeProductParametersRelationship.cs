﻿// <auto-generated/>
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProductParametersRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_parameters_products_product_id",
                schema: "catalog",
                table: "parameters");

            migrationBuilder.DropIndex(
                name: "ix_parameters_product_id",
                schema: "catalog",
                table: "parameters");

            migrationBuilder.DropColumn(
                name: "product_id",
                schema: "catalog",
                table: "parameters");

            migrationBuilder.CreateTable(
                name: "product_parameters",
                schema: "catalog",
                columns: table => new
                {
                    parameter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_parameters", x => new { x.parameter_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_product_parameters_parameters_parameters_id",
                        column: x => x.parameter_id,
                        principalSchema: "catalog",
                        principalTable: "parameters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_parameters_products_product_id",
                        column: x => x.product_id,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_parameters_product_id",
                schema: "catalog",
                table: "product_parameters",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_parameters",
                schema: "catalog");

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                schema: "catalog",
                table: "parameters",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_parameters_product_id",
                schema: "catalog",
                table: "parameters",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "fk_parameters_products_product_id",
                schema: "catalog",
                table: "parameters",
                column: "product_id",
                principalSchema: "catalog",
                principalTable: "products",
                principalColumn: "id");
        }
    }
}
