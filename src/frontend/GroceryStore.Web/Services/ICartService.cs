namespace GroceryStore.Web.Services;

public interface ICartService
{
    IReadOnlyList<CartItem> GetItems();
    int GetCount();
    void Add(CartItem item, int quantity = 1);
    void UpdateQuantity(Guid productId, int quantity);
    void Remove(Guid productId);
    void Clear();
}
