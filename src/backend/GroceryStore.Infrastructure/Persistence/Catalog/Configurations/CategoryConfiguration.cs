using GroceryStore.Domain.Entities;
using GroceryStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryStore.Infrastructure.Persistence.Catalog.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        // ── Primary key ──
        builder.HasKey(c => c.Id);

        // ── Scalar properties ──
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.SortOrder)
            .IsRequired();

        builder.Property(c => c.ParentCategoryId);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.IconName)
            .HasMaxLength(50);

        // ── Auditing ──
        builder.Property(c => c.CreatedOnUtc)
            .IsRequired();

        builder.Property(c => c.ModifiedOnUtc);

        // ── Value objects ──
        builder.ComplexProperty(c => c.Slug, slugBuilder =>
        {
            slugBuilder.Property(s => s.Value)
                .HasColumnName("Slug")
                .IsRequired()
                .HasMaxLength(120);
        });

        builder.ComplexProperty(c => c.Seo, seoBuilder =>
        {
            seoBuilder.Property(s => s.MetaTitle)
                .HasColumnName("SeoMetaTitle")
                .HasMaxLength(60);

            seoBuilder.Property(s => s.MetaDescription)
                .HasColumnName("SeoMetaDescription")
                .HasMaxLength(160);
        });

        // ── Indexes ──
        builder.HasIndex(c => c.ParentCategoryId)
            .HasDatabaseName("IX_Categories_ParentCategoryId");

        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Categories_IsActive");

        // ── Self-referencing relationship (optional parent) ──
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // ── Ignore domain events ──
        builder.Ignore(c => c.DomainEvents);
    }
}
