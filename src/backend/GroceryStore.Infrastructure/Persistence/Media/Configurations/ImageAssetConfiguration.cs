using GroceryStore.Domain.Entities.Media;
using GroceryStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryStore.Infrastructure.Persistence.Media.Configurations;

public sealed class ImageAssetConfiguration : IEntityTypeConfiguration<ImageAsset>
{
    public void Configure(EntityTypeBuilder<ImageAsset> builder)
    {
        builder.ToTable("ImageAssets");

        // ── Primary key ──
        builder.HasKey(a => a.Id);

        // ── ImageId value object → Guid column ──
        builder.Property(a => a.ImageId)
            .HasConversion(
                id => id.Value,
                value => ImageId.Create(value))
            .HasColumnName("ImageId")
            .IsRequired();

        // ── Scalar properties ──
        builder.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Url)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.AltText)
            .HasMaxLength(200);

        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // ── Auditing ──
        builder.Property(a => a.CreatedOnUtc)
            .IsRequired();

        builder.Property(a => a.ModifiedOnUtc);

        // ── ImageMetadata (owned value object) ──
        builder.ComplexProperty(a => a.Metadata, metaBuilder =>
        {
            metaBuilder.Property(m => m.OriginalFileName)
                .HasColumnName("MetadataOriginalFileName")
                .IsRequired()
                .HasMaxLength(260);

            metaBuilder.Property(m => m.ContentType)
                .HasColumnName("MetadataContentType")
                .IsRequired()
                .HasMaxLength(100);

            metaBuilder.Property(m => m.FileSizeBytes)
                .HasColumnName("MetadataFileSizeBytes")
                .IsRequired();

            metaBuilder.Property(m => m.WidthPx)
                .HasColumnName("MetadataWidthPx");

            metaBuilder.Property(m => m.HeightPx)
                .HasColumnName("MetadataHeightPx");
        });

        // ── Indexes ──
        builder.HasIndex(a => a.ImageId)
            .IsUnique()
            .HasDatabaseName("IX_ImageAssets_ImageId");

        builder.HasIndex(a => a.IsDeleted)
            .HasDatabaseName("IX_ImageAssets_IsDeleted");

        // ── Query filter for soft delete ──
        builder.HasQueryFilter(a => !a.IsDeleted);

        // ── Ignore domain events ──
        builder.Ignore(a => a.DomainEvents);
    }
}
