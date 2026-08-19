using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZeissAssessment.Domain.Entities;

namespace ZeissAssessment.Infrastructure.Persistence.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Id)
            .HasDefaultValueSql("NEXT VALUE FOR dbo.ProductIdSequence")
            .ValueGeneratedOnAdd();

        builder.HasIndex(p => p.Id).IsUnique();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);

        builder.OwnsOne(p => p.Stock, stock =>
        {
            stock.Property(s => s.Quantity)
                .HasColumnName("Quantity")
                .IsRequired();
        });
    }
}