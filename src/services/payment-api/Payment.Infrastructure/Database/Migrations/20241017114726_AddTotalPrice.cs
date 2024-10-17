﻿// <auto-generated/>
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                schema: "payment",
                table: "invoice_lines",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "total_currency",
                schema: "payment",
                table: "invoice_lines",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "total_amount",
                schema: "payment",
                table: "invoice_lines");

            migrationBuilder.DropColumn(
                name: "total_currency",
                schema: "payment",
                table: "invoice_lines");
        }
    }
}
