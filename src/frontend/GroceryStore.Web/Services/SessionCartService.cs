using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GroceryStore.Web.Services;

public sealed class SessionCartService : ICartService
{
    private const string SessionKey = "grocery.cart.v1";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionCartService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public IReadOnlyList<CartItem> GetItems()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null) return [];

        var json = session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json)) return [];

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    public int GetCount() => GetItems().Sum(x => x.Quantity);

    public void Add(CartItem item, int quantity = 1)
    {
        if (quantity < 1) quantity = 1;

        var items = GetItems().ToList();
        var existing = items.FirstOrDefault(x => x.ProductId == item.ProductId);

        if (existing is null)
        {
            item.Quantity = quantity;
            items.Add(item);
        }
        else
        {
            existing.Quantity += quantity;
            existing.Currency = item.Currency;
            existing.Unit = item.Unit;
            existing.Slug = item.Slug;
            existing.Name = item.Name;
            existing.Brand = item.Brand;
            existing.Sku = item.Sku;
            existing.UnitPrice = item.UnitPrice;
        }

        Save(items);
    }

    public void UpdateQuantity(Guid productId, int quantity)
    {
        var items = GetItems().ToList();
        var existing = items.FirstOrDefault(x => x.ProductId == productId);
        if (existing is null) return;

        if (quantity <= 0)
            items.Remove(existing);
        else
            existing.Quantity = quantity;

        Save(items);
    }

    public void Remove(Guid productId)
    {
        var items = GetItems().ToList();
        items.RemoveAll(x => x.ProductId == productId);
        Save(items);
    }

    public void Clear() => Save([]);

    private void Save(List<CartItem> items)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null) return;

        var json = JsonSerializer.Serialize(items);
        session.SetString(SessionKey, json);
    }
}
