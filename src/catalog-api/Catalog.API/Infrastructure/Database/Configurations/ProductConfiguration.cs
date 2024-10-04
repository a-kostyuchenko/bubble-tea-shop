using Catalog.API.Entities.Products;
using Catalog.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDefaults.Domain;

namespace Catalog.API.Infrastructure.Database.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(TableNames.Products);
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasConversion(category => category.Name, name => Category.FromName(name))
            .HasMaxLength(100);

        builder.ComplexProperty(b => b.Price, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");

            priceBuilder.Property(p => p.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("currency");
        });

        builder.HasMany(b => b.Ingredients)
            .WithMany()
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable(TableNames.ProductIngredients);
                
                joinBuilder.Property("IngredientsId").HasColumnName("ingredient_id");
            });

        builder.OwnsMany(p => p.Parameters, parameterBuilder =>
        {
            parameterBuilder.WithOwner();

            parameterBuilder.ToTable(TableNames.Parameters);
            
            parameterBuilder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();
            
            parameterBuilder.OwnsMany(p => p.Options, optionBuilder =>
            {
                optionBuilder.WithOwner();
                
                optionBuilder.ToTable(TableNames.Options);

                optionBuilder.Property(o => o.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                optionBuilder.OwnsOne(o => o.ExtraPrice, priceBuilder =>
                {
                    priceBuilder.Property(p => p.Amount)
                        .HasPrecision(10, 2)
                        .HasColumnName("amount");

                    priceBuilder.Property(p => p.Currency)
                        .IsRequired()
                        .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                        .HasMaxLength(3)
                        .HasColumnName("currency");
                });
            });
        });
    }
}
