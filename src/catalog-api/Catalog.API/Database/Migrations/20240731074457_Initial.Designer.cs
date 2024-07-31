﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Catalog.API.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.API.Database.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    [Migration("20240731074457_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("catalog")
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BubbleTeaIngredient", b =>
                {
                    b.Property<Guid>("BubbleTeaId")
                        .HasColumnType("uuid")
                        .HasColumnName("bubble_tea_id");

                    b.Property<Guid>("IngredientsId")
                        .HasColumnType("uuid")
                        .HasColumnName("ingredients_id");

                    b.HasKey("BubbleTeaId", "IngredientsId")
                        .HasName("pk_bubble_tea_ingredients");

                    b.HasIndex("IngredientsId")
                        .HasDatabaseName("ix_bubble_tea_ingredients_ingredients_id");

                    b.ToTable("bubble_tea_ingredients", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Entities.BubbleTeas.BubbleTea", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("name");

                    b.Property<string>("TeaType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("tea_type");

                    b.ComplexProperty<Dictionary<string, object>>("Price", "Catalog.API.Entities.BubbleTeas.BubbleTea.Price#Money", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<decimal>("Amount")
                                .HasPrecision(10, 2)
                                .HasColumnType("numeric(10,2)")
                                .HasColumnName("amount");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasMaxLength(3)
                                .HasColumnType("character varying(3)")
                                .HasColumnName("currency");
                        });

                    b.HasKey("Id")
                        .HasName("pk_bubble_teas");

                    b.ToTable("bubble_teas", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Entities.Ingredients.Ingredient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_ingredients");

                    b.ToTable("ingredients", "catalog");
                });

            modelBuilder.Entity("BubbleTeaIngredient", b =>
                {
                    b.HasOne("Catalog.API.Entities.BubbleTeas.BubbleTea", null)
                        .WithMany()
                        .HasForeignKey("BubbleTeaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_bubble_tea_ingredients_bubble_teas_bubble_tea_id");

                    b.HasOne("Catalog.API.Entities.Ingredients.Ingredient", null)
                        .WithMany()
                        .HasForeignKey("IngredientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_bubble_tea_ingredients_ingredients_ingredients_id");
                });
#pragma warning restore 612, 618
        }
    }
}
