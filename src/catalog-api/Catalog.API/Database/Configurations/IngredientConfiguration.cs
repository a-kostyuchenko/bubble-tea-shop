using Catalog.API.Database.Constants;
using Catalog.API.Entities.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.API.Database.Configurations;

internal sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable(TableNames.Ingredients);
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(300);
    }
}
