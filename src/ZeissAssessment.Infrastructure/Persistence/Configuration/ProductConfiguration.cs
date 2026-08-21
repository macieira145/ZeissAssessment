using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

        builder.ToTable(t => t.HasCheckConstraint("CK_Product_Id_Range", "[Id] BETWEEN 100000 AND 999999"));

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);

        builder.Property(p => p.Price).HasPrecision(18, 2);

        builder.Property(p => p.Created)
            .IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(p => p.Updated).IsRequired();

        builder.OwnsOne(p => p.Stock, stock =>
        {
            stock.Property(s => s.Quantity)
                .HasColumnName("Quantity")
                .IsRequired();
        });
    }
}