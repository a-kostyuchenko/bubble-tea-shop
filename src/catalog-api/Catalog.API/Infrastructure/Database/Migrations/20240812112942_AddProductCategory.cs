﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.API.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                schema: "catalog",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                schema: "catalog",
                table: "products");
        }
    }
}
