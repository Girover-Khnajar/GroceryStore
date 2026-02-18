using GroceryStore.Domain.Entities.Catalog;
using GroceryStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryStore.Infrastructure.Persistence.Catalog.Configurations;

public sealed class ProductImageRefConfiguration : IEntityTypeConfiguration<ProductImageRef>
{
    public void Configure(EntityTypeBuilder<ProductImageRef> builder)
    {
        builder.ToTable("ProductImageRefs");

        // ── Primary key ──
        builder.HasKey(r => r.Id);

        // ── Shadow FK set by Product configuration ──
        builder.Property<Guid>("ProductId")
            .IsRequired();

        // ── ImageId value object → Guid column ──
        builder.Property(r => r.ImageId)
            .HasConversion(
                id => id.Value,
                value => ImageId.Create(value))
            .HasColumnName("ImageId")
            .IsRequired();

        builder.Property(r => r.IsPrimary)
            .IsRequired();

        builder.Property(r => r.SortOrder)
            .IsRequired();

        builder.Property(r => r.AltText)
            .HasMaxLength(200);

        // ── Indexes ──
        builder.HasIndex("ProductId", "ImageId")
            .IsUnique()
            .HasDatabaseName("IX_ProductImageRefs_ProductId_ImageId");
    }
}
