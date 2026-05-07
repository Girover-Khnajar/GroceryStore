using GroceryStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GroceryStore.Infrastructure.Persistence.Marketing.Configurations;

public sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("testimonials");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ClientName)
            .HasColumnName("client_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(t => t.ClientTitle)
            .HasColumnName("client_title")
            .HasMaxLength(150);

        builder.Property(t => t.ClientCompany)
            .HasColumnName("client_company")
            .HasMaxLength(150);

        builder.Property(t => t.ClientImage)
            .HasColumnName("client_image")
            .HasMaxLength(255);

        builder.Property(t => t.Rating)
            .HasColumnName("rating")
            .HasColumnType("tinyint");

        builder.Property(t => t.TestimonialText)
            .HasColumnName("testimonial")
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(t => t.SortOrder)
            .HasColumnName("sort_order")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(t => t.CreatedOnUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.ModifiedOnUtc)
            .HasColumnName("updated_at");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_testimonials_is_active");

        builder.HasIndex(t => t.SortOrder)
            .HasDatabaseName("IX_testimonials_sort_order");

        builder.Ignore(t => t.DomainEvents);
    }
}
