using GroceryStore.Domain.Common.DomainEvents;
using GroceryStore.Domain.Exceptions;
using GroceryStore.Domain.Interfaces;

namespace GroceryStore.Domain.Entities;

public sealed class Testimonial : AuditableEntity, IAggregateRoot
{
    private Testimonial() { }

    public string ClientName { get; private set; } = string.Empty;
    public string? ClientTitle { get; private set; }
    public string? ClientCompany { get; private set; }
    public string? ClientImage { get; private set; }
    public byte? Rating { get; private set; }
    public string TestimonialText { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static Testimonial Create(
        string clientName,
        string testimonialText,
        string? clientTitle = null,
        string? clientCompany = null,
        string? clientImage = null,
        byte? rating = null,
        int sortOrder = 0,
        bool isActive = true)
    {
        Validate(clientName, testimonialText, rating, sortOrder);

        return new Testimonial
        {
            ClientName = clientName.Trim(),
            TestimonialText = testimonialText.Trim(),
            ClientTitle = clientTitle?.Trim(),
            ClientCompany = clientCompany?.Trim(),
            ClientImage = clientImage?.Trim(),
            Rating = rating,
            SortOrder = sortOrder,
            IsActive = isActive
        };
    }

    public void Update(
        string clientName,
        string testimonialText,
        string? clientTitle,
        string? clientCompany,
        string? clientImage,
        byte? rating,
        int sortOrder,
        bool isActive)
    {
        Validate(clientName, testimonialText, rating, sortOrder);

        ClientName = clientName.Trim();
        TestimonialText = testimonialText.Trim();
        ClientTitle = clientTitle?.Trim();
        ClientCompany = clientCompany?.Trim();
        ClientImage = clientImage?.Trim();
        Rating = rating;
        SortOrder = sortOrder;
        IsActive = isActive;
        Touch();
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
        Touch();
    }

    public void SetSortOrder(int sortOrder)
    {
        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 100_000);
        SortOrder = sortOrder;
        Touch();
    }

    private static void Validate(string clientName, string testimonialText, byte? rating, int sortOrder)
    {
        ValidationException.ThrowIfNullOrWhiteSpace(clientName);
        ValidationException.ThrowIfTooLong(clientName, 150);

        ValidationException.ThrowIfNullOrWhiteSpace(testimonialText);
        ValidationException.ThrowIfTooLong(testimonialText, 4000);

        if (rating.HasValue && (rating.Value < 1 || rating.Value > 5))
            throw new ValidationException("Rating must be between 1 and 5.");

        ValidationException.ThrowIfOutOfRange(sortOrder, 0, 100_000);
    }
}
