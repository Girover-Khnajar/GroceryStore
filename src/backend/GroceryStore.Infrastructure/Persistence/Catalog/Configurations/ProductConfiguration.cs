using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Entities.Catalog;
using GroceryStore.Domain.Enums;
using GroceryStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryStore.Infrastructure.Persistence.Catalog.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        // ── Primary key ──
        builder.HasKey(p => p.Id);

        // ── Scalar properties ──
        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(p => p.ShortDescription)
            .HasMaxLength(300);

        builder.Property(p => p.LongDescription)
            .HasMaxLength(5000);

        builder.Property(p => p.Unit)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.IsFeatured)
            .IsRequired();

        builder.Property(p => p.SortOrder)
            .IsRequired();

        builder.Property(p => p.Sku)
            .HasMaxLength(60);

        builder.Property(p => p.Barcode)
            .HasMaxLength(40);

        builder.Property(p => p.Brand)
            .HasMaxLength(80);

        builder.Property(p => p.OriginCountryCode)
            .HasMaxLength(2)
            .IsUnicode(false);

        builder.Property(p => p.IsOrganic);
        builder.Property(p => p.IsHalal);
        builder.Property(p => p.IsVegan);

        builder.Property(p => p.Ingredients)
            .HasMaxLength(4000);

        builder.Property(p => p.Storage)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.NetWeight)
            .HasPrecision(18, 4);

        builder.Property(p => p.NetWeightUnit)
            .HasConversion<string>()
            .HasMaxLength(20);

        // ── Auditing ──
        builder.Property(p => p.CreatedOnUtc)
            .IsRequired();

        builder.Property(p => p.ModifiedOnUtc);

        // ── Value objects ──
        builder.ComplexProperty(p => p.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .IsRequired()
                .HasPrecision(18, 2);

            priceBuilder.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false);
        });

        builder.ComplexProperty(p => p.Slug, slugBuilder =>
        {
            slugBuilder.Property(s => s.Value)
                .HasColumnName("Slug")
                .IsRequired()
                .HasMaxLength(120);
        });

        builder.ComplexProperty(p => p.Seo, seoBuilder =>
        {
            seoBuilder.Property(s => s.MetaTitle)
                .HasColumnName("SeoMetaTitle")
                .HasMaxLength(60);

            seoBuilder.Property(s => s.MetaDescription)
                .HasColumnName("SeoMetaDescription")
                .HasMaxLength(160);
        });

        // ── NutritionFacts (optional owned type) ──
        builder.OwnsOne(p => p.Nutrition, nutritionBuilder =>
        {
            nutritionBuilder.Property(n => n.CaloriesKcal)
                .HasColumnName("NutritionCaloriesKcal")
                .HasPrecision(10, 2);

            nutritionBuilder.Property(n => n.ProteinG)
                .HasColumnName("NutritionProteinG")
                .HasPrecision(10, 2);

            nutritionBuilder.Property(n => n.CarbsG)
                .HasColumnName("NutritionCarbsG")
                .HasPrecision(10, 2);

            nutritionBuilder.Property(n => n.FatG)
                .HasColumnName("NutritionFatG")
                .HasPrecision(10, 2);

            nutritionBuilder.Property(n => n.SaltG)
                .HasColumnName("NutritionSaltG")
                .HasPrecision(10, 2);
        });

        // ── Tags & Allergens (primitive collections) ──
        builder.Property(p => p.Tags)
            .HasColumnName("Tags");

        builder.Property(p => p.Allergens)
            .HasColumnName("Allergens");

        // ── Navigation: ImageRefs (owned collection) ──
        builder.HasMany(p => p.ImageRefs)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.ImageRefs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ── Relationship: Category ──
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ──
        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Products_IsActive");

        builder.HasIndex(p => p.Sku)
            .HasDatabaseName("IX_Products_Sku");

        builder.HasIndex(p => p.Barcode)
            .HasDatabaseName("IX_Products_Barcode");

        // ── Ignore domain events ──
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.PrimaryImageRef);
    }
}
