// using FluentAssertions;
// using GroceryStore.Models;
// using GroceryStore.Services.Mock;
// using GroceryStore.Tests.Helpers;

// namespace GroceryStore.Tests.Services;

// public class MockCategoryServiceTests
// {
//     private readonly MockCategoryService _sut = new( );


//     [Fact]
//     public async Task GetCategories_ReturnsOnlyActive()
//     {
//         MockDbFactory.Reset( );

//         var categories = await _sut.GetCategoriesAsync( );

//         categories.Should( ).HaveCount(2);  // Dairy is inactive
//         categories.Should( ).OnlyContain(c => c.IsActive);
//     }

//     [Fact]
//     public async Task GetCategories_OrderedByDisplayOrder()
//     {
//         MockDbFactory.Reset( );

//         var categories = await _sut.GetCategoriesAsync( );

//         var orders = categories.Select(c => c.DisplayOrder).ToList( );
//         orders.Should( ).BeInAscendingOrder( );
//     }


//     [Fact]
//     public async Task GetAllCategories_ReturnsAll_IncludingInactive()
//     {
//         MockDbFactory.Reset( );

//         var categories = await _sut.GetAllCategoriesAsync( );

//         categories.Should( ).HaveCount(3);
//     }


//     [Fact]
//     public async Task GetCategoryById_ExistingId_ReturnsCategory()
//     {
//         MockDbFactory.Reset( );

//         var category = await _sut.GetCategoryByIdAsync(2);

//         category.Should( ).NotBeNull( );
//         category!.Name.Should( ).Be("Fruits");
//     }

//     [Fact]
//     public async Task GetCategoryById_UnknownId_ReturnsNull()
//     {
//         MockDbFactory.Reset( );

//         var category = await _sut.GetCategoryByIdAsync(999);

//         category.Should( ).BeNull( );
//     }


//     [Fact]
//     public async Task GetCategoryBySlug_ExistingSlug_ReturnsCategory()
//     {
//         MockDbFactory.Reset( );

//         var category = await _sut.GetCategoryBySlugAsync("vegetables");

//         category.Should( ).NotBeNull( );
//         category!.Id.Should( ).Be(1);
//     }

//     [Fact]
//     public async Task GetCategoryBySlug_UnknownSlug_ReturnsNull()
//     {
//         MockDbFactory.Reset( );

//         var category = await _sut.GetCategoryBySlugAsync("not-a-real-slug");

//         category.Should( ).BeNull( );
//     }


//     [Fact]
//     public async Task CreateCategory_AssignsNewId_AndPersists()
//     {
//         MockDbFactory.Reset( );
//         var before = MockDb.Categories.Count;

//         var created = await _sut.CreateCategoryAsync(new Category
//         {
//             Name = "Bakery",
//             Slug = "bakery",
//             IsActive = true,
//             DisplayOrder = 10
//         });

//         created.Id.Should( ).BeGreaterThan(0);
//         MockDb.Categories.Should( ).HaveCount(before + 1);
//     }

//     [Fact]
//     public async Task CreateCategory_IdIsUniqueAndHigherThanExisting()
//     {
//         MockDbFactory.Reset( );
//         var maxId = MockDb.Categories.Max(c => c.Id);

//         var created = await _sut.CreateCategoryAsync(new Category { Name = "New",Slug = "new" });

//         created.Id.Should( ).BeGreaterThan(maxId);
//     }


//     [Fact]
//     public async Task UpdateCategory_ChangesExistingRecord()
//     {
//         MockDbFactory.Reset( );

//         var updated = new Category { Id = 1,Name = "Organic Vegetables",Slug = "vegetables",IsActive = true,DisplayOrder = 1 };
//         await _sut.UpdateCategoryAsync(updated);

//         var inDb = MockDb.Categories.First(c => c.Id == 1);
//         inDb.Name.Should( ).Be("Organic Vegetables");
//     }


//     [Fact]
//     public async Task DeleteCategory_ExistingId_RemovesAndReturnsTrue()
//     {
//         MockDbFactory.Reset( );

//         var result = await _sut.DeleteCategoryAsync(1);

//         result.Should( ).BeTrue( );
//         MockDb.Categories.Should( ).NotContain(c => c.Id == 1);
//     }

//     [Fact]
//     public async Task DeleteCategory_UnknownId_ReturnsFalse()
//     {
//         MockDbFactory.Reset( );

//         var result = await _sut.DeleteCategoryAsync(9999);

//         result.Should( ).BeFalse( );
//     }
// }
