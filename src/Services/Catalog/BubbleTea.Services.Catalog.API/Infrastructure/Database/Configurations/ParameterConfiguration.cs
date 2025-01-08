using BubbleTea.Services.Catalog.API.Entities.Parameters;
using BubbleTea.Services.Catalog.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleTea.Services.Catalog.API.Infrastructure.Database.Configurations;

internal sealed class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> builder)
    {
        builder.ToTable(TableNames.Parameters);
        
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasMany(p => p.Options)
            .WithOne()
            .HasForeignKey("parameter_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
