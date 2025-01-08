using BubbleTea.Services.Cart.API.Entities.Carts;
using BubbleTea.Services.Cart.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleTea.Services.Cart.API.Infrastructure.Database.Configurations;

internal sealed class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> builder)
    {
        builder.ToTable(TableNames.Parameters);
        
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(p => p.SelectedOption)
            .WithOne()
            .HasForeignKey<Option>("parameter_id")
            .IsRequired();
        
        builder.Navigation(p => p.SelectedOption).AutoInclude();
    }
}
