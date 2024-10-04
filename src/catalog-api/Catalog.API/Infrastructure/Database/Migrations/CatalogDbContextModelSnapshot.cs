﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Catalog.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Catalog.API.Infrastructure.Database.Migrations
{
    [DbContext(typeof(CatalogDbContext))]
    partial class CatalogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("catalog")
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Catalog.API.Entities.Ingredients.Ingredient", b =>
                {
                    b.Property<Guid>("Id")
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

            modelBuilder.Entity("Catalog.API.Entities.Parameters.Option", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("name");

                    b.Property<double>("Value")
                        .HasColumnType("double precision")
                        .HasColumnName("value");

                    b.Property<Guid>("parameter_id")
                        .HasColumnType("uuid")
                        .HasColumnName("parameter_id");

                    b.ComplexProperty<Dictionary<string, object>>("ExtraPrice", "Catalog.API.Entities.Parameters.Option.ExtraPrice#Money", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<decimal>("Amount")
                                .HasPrecision(10, 2)
                                .HasColumnType("numeric(10,2)")
                                .HasColumnName("extra_price");

                            b1.Property<string>("Currency")
                                .IsRequired()
                                .HasMaxLength(3)
                                .HasColumnType("character varying(3)")
                                .HasColumnName("currency");
                        });

                    b.HasKey("Id")
                        .HasName("pk_options");

                    b.HasIndex("parameter_id")
                        .HasDatabaseName("ix_options_parameter_id");

                    b.ToTable("options", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Entities.Parameters.Parameter", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("name");

                    b.Property<Guid?>("ProductId")
                        .HasColumnType("uuid")
                        .HasColumnName("product_id");

                    b.HasKey("Id")
                        .HasName("pk_parameters");

                    b.HasIndex("ProductId")
                        .HasDatabaseName("ix_parameters_product_id");

                    b.ToTable("parameters", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Entities.Products.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("category");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("name");

                    b.ComplexProperty<Dictionary<string, object>>("Price", "Catalog.API.Entities.Products.Product.Price#Money", b1 =>
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
                        .HasName("pk_products");

                    b.ToTable("products", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Infrastructure.Inbox.InboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_inbox_messages");

                    b.ToTable("inbox_messages", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Infrastructure.Inbox.InboxMessageConsumer", b =>
                {
                    b.Property<Guid>("InboxMessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("inbox_message_id");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("name");

                    b.HasKey("InboxMessageId", "Name")
                        .HasName("pk_inbox_message_consumers");

                    b.ToTable("inbox_message_consumers", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Infrastructure.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("jsonb")
                        .HasColumnName("content");

                    b.Property<string>("Error")
                        .HasColumnType("text")
                        .HasColumnName("error");

                    b.Property<DateTime>("OccurredOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("occurred_on_utc");

                    b.Property<DateTime?>("ProcessedOnUtc")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("processed_on_utc");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");

                    b.HasKey("Id")
                        .HasName("pk_outbox_messages");

                    b.ToTable("outbox_messages", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Infrastructure.Outbox.OutboxMessageConsumer", b =>
                {
                    b.Property<Guid>("OutboxMessageId")
                        .HasColumnType("uuid")
                        .HasColumnName("outbox_message_id");

                    b.Property<string>("Name")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("name");

                    b.HasKey("OutboxMessageId", "Name")
                        .HasName("pk_outbox_message_consumers");

                    b.ToTable("outbox_message_consumers", "catalog");
                });

            modelBuilder.Entity("IngredientProduct", b =>
                {
                    b.Property<Guid>("IngredientsId")
                        .HasColumnType("uuid")
                        .HasColumnName("ingredient_id");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid")
                        .HasColumnName("product_id");

                    b.HasKey("IngredientsId", "ProductId")
                        .HasName("pk_product_ingredients");

                    b.HasIndex("ProductId")
                        .HasDatabaseName("ix_product_ingredients_product_id");

                    b.ToTable("product_ingredients", "catalog");
                });

            modelBuilder.Entity("Catalog.API.Entities.Parameters.Option", b =>
                {
                    b.HasOne("Catalog.API.Entities.Parameters.Parameter", null)
                        .WithMany("Options")
                        .HasForeignKey("parameter_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_options_parameters_parameter_id");
                });

            modelBuilder.Entity("Catalog.API.Entities.Parameters.Parameter", b =>
                {
                    b.HasOne("Catalog.API.Entities.Products.Product", null)
                        .WithMany("Parameters")
                        .HasForeignKey("ProductId")
                        .HasConstraintName("fk_parameters_products_product_id");
                });

            modelBuilder.Entity("IngredientProduct", b =>
                {
                    b.HasOne("Catalog.API.Entities.Ingredients.Ingredient", null)
                        .WithMany()
                        .HasForeignKey("IngredientsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_product_ingredients_ingredients_ingredients_id");

                    b.HasOne("Catalog.API.Entities.Products.Product", null)
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_product_ingredients_products_product_id");
                });

            modelBuilder.Entity("Catalog.API.Entities.Parameters.Parameter", b =>
                {
                    b.Navigation("Options");
                });

            modelBuilder.Entity("Catalog.API.Entities.Products.Product", b =>
                {
                    b.Navigation("Parameters");
                });
#pragma warning restore 612, 618
        }
    }
}
