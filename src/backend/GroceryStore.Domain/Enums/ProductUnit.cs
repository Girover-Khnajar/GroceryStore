namespace GroceryStore.Domain.Enums;

/// <summary>
/// Unit used for display/price.
/// </summary>
public enum ProductUnit
{
    Piece = 1,
    Kg = 2,
    Gram = 3,
    Liter = 4,
    Ml = 5,
    Pack = 6,
    Bottle = 7,
    Can = 8,
    Box = 9
}

/// <summary>
/// Storage recommendation.
/// </summary>
public enum StorageCondition
{
    None = 0,
    RoomTemperature = 1,
    Refrigerated = 2,
    Frozen = 3
}