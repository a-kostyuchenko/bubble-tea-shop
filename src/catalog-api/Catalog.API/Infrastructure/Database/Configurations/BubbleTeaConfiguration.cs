using Catalog.API.Entities.BubbleTeas;
using Catalog.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.API.Infrastructure.Database.Configurations;

internal sealed class BubbleTeaConfiguration : IEntityTypeConfiguration<BubbleTea>
{
    public void Configure(EntityTypeBuilder<BubbleTea> builder)
    {
        builder.ToTable(TableNames.BubbleTeas);
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(b => b.TeaType)
            .IsRequired()
            .HasConversion(type => type.Name, name => TeaType.FromName(name))
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
                joinBuilder.ToTable(TableNames.BubbleTeaIngredients);
                
                joinBuilder.Property("IngredientsId").HasColumnName("ingredient_id");
            });
    }
}
