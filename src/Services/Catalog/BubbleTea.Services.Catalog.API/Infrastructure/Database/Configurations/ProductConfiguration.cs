using BubbleTea.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BubbleTea.Services.Catalog.API.Entities.Products;
using BubbleTea.Services.Catalog.API.Infrastructure.Database.Constants;

namespace BubbleTea.Services.Catalog.API.Infrastructure.Database.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(TableNames.Products);
        
        builder.HasKey(p => p.Id);
        
        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasIndex(p => new { p.Name, p.Description })
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(400);
        
        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasConversion(category => category.Name, name => Category.FromName(name))
            .HasMaxLength(100);

        builder.ComplexProperty(p => p.Price, priceBuilder =>
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

        builder.HasMany(p => p.Ingredients)
            .WithMany()
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable(TableNames.ProductIngredients);
                
                joinBuilder.Property("IngredientsId").HasColumnName("ingredient_id");
            });
        
        builder.HasMany(p => p.Parameters)
            .WithMany()
            .UsingEntity(joinBuilder =>
            {
                joinBuilder.ToTable(TableNames.ProductParameters);
                
                joinBuilder.Property("ParametersId").HasColumnName("parameter_id");
            });
    }
}
