using GroceryStore.Ui.Models;

namespace GroceryStore.Ui.Data;

public static class Db
{
    public static List<Category> Categories { get; set; } =
    [
        new()
        {
            Slug = "vegetables",
            Name = "خضروات طازجة",
            ImageUrl = "https://picsum.photos/seed/category-vegetables/900/675",
            Alt = "خضروات طازجة"
        },
        new()
        {
            Slug = "fruits",
            Name = "فواكه",
            ImageUrl = "https://picsum.photos/seed/category-fruits/900/675",
            Alt = "فواكه"
        },
        new()
        {
            Slug = "dairy",
            Name = "منتجات الألبان",
            ImageUrl = "https://picsum.photos/seed/category-dairy/900/675",
            Alt = "منتجات الألبان"
        },
        new()
        {
            Slug = "beverages",
            Name = "مشروبات",
            ImageUrl = "https://picsum.photos/seed/category-beverages/900/675",
            Alt = "مشروبات"
        },
        new()
        {
            Slug = "canned",
            Name = "معلبات",
            ImageUrl = "https://picsum.photos/seed/category-canned/900/675",
            Alt = "معلبات"
        }
    ];

    public static List<Product> Products { get; set; } = [
            new()
            {
                Name = "طماطم طازجة",
                Category = "خضروات طازجة",
                CurrentPrice = 2.5m,
                Unit = "kg",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/fresh-tomatoes/900/900"
            },
            new ()
            {
                Name = "تفاح أحمر",
                Category = "فواكه",
                CurrentPrice = 3.2m,
                Unit = "kg",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/red-apple/900/900"
            },
            new ()
            {
                Name = "موز",
                Category = "فواكه",
                CurrentPrice = 2m,
                Unit = "kg",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/banana/900/900"
            },
            new ()
            {
                Name = "عصير برتقال",
                Category = "مشروبات",
                CurrentPrice = 2.8m,
                Unit = "لتر",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/orange-juice/900/900"
            },
            new ()
            {
                Name = "جزر",
                Category = "خضروات طازجة",
                CurrentPrice = 1.8m,
                Unit = "kg",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/carrots/900/900"
            },
            new ()
            {
                Name = "خس",
                Category = "خضروات طازجة",
                CurrentPrice = 2m,
                Unit = "kg",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/lettuce/900/900"
            },
            new ()
            {
                Name = "فراولة",
                Category = "فواكه",
                CurrentPrice = 4.5m,
                Unit = "kg",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/strawberry/900/900"
            },
            new()
            {
                Name = "عصير تفاح",
                Category = "مشروبات",
                CurrentPrice = 2.5m,
                Unit = "لتر",
                Currency = "IQD",
                ImageUrl = "https://picsum.photos/seed/apple-juice/900/900"
            }
        ];

    public static List<Slide> Slides { get; set; } = [
            new Slide("https://picsum.photos/seed/slide1/1200/400", "تخفيضات الصيف", "/deals/summer"),
            new Slide("https://picsum.photos/seed/slide2/1200/400", "منتجات عضوية طازجة", "/products/organic"),
            new Slide("https://picsum.photos/seed/slide3/1200/400", "أفضل العروض الأسبوعية", "/deals/weekly")
        ];
}