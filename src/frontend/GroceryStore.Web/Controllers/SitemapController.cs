using System.Text;
using System.Xml;
using CQRS.Abstractions.Messaging;
using GroceryStore.Application.Categories.Dtos;
using GroceryStore.Application.Categories.Queries;
using GroceryStore.Application.Common;
using GroceryStore.Application.Products.Dtos;
using GroceryStore.Application.Products.Queries.GetProducts;
using GroceryStore.Web.Services.Localization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Web.Controllers;

/// <summary>
/// Generates an XML sitemap with hreflang alternate links for all supported cultures.
/// Accessible at /sitemap.xml
/// </summary>
public class SitemapController : Controller
{
    private readonly IMessageDispatcher _dispatcher;

    public SitemapController(IMessageDispatcher dispatcher)
        => _dispatcher = dispatcher;

    [AcceptVerbs("GET", "HEAD")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var cultures = CultureHelper.SupportedCultureCodes;

        // IMPORTANT: These queries typically share the same scoped DbContext under the hood.
        // Running them concurrently can trigger EF Core's concurrency detector.
        var productsResult = await _dispatcher.QueryAsync<GetProductsQuery, PagedResult<ProductListItemDto>>(
            new GetProductsQuery(null, null, null, null, true, null, null, null, 1, 5000), ct);
        var categoriesResult = await _dispatcher.QueryAsync<GetAllActiveCategoriesQuery, IReadOnlyList<CategoryDto>>(
            new GetAllActiveCategoriesQuery(), ct);

        var products = productsResult.IsSuccess ? productsResult.Value!.Items : [];
        var categories = categoriesResult.IsSuccess ? categoriesResult.Value! : [];

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            Async = true
        };

        using var ms = new MemoryStream();
        await using (var writer = XmlWriter.Create(ms, settings))
        {
            await writer.WriteStartDocumentAsync();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
            writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");

            // Static pages
            var staticPages = new[]
            {
                new { Loc = "/", Priority = "1.0",  ChangeFreq = "daily"   },
                new { Loc = "/Products",   Priority = "0.9",  ChangeFreq = "daily"   },
                new { Loc = "/Categories", Priority = "0.8",  ChangeFreq = "weekly"  },
                new { Loc = "/contact",    Priority = "0.5",  ChangeFreq = "monthly" },
            };

            foreach (var page in staticPages)
                WriteUrl(writer, baseUrl, page.Loc, cultures, page.Priority, page.ChangeFreq);

            // Category pages
            foreach (var cat in categories)
                WriteUrl(writer, baseUrl, $"/Categories/{cat.Slug}", cultures, "0.7", "weekly");

            // Product pages
            foreach (var prod in products)
                WriteUrl(writer, baseUrl, $"/Products/{prod.Slug}", cultures, "0.8", "weekly");

            writer.WriteEndElement(); // urlset
            await writer.WriteEndDocumentAsync();
        }

        return File(ms.ToArray(), "application/xml; charset=utf-8");
    }

    private static void WriteUrl(
        XmlWriter writer,
        string baseUrl,
        string path,
        IReadOnlyList<string> cultures,
        string priority,
        string changeFreq)
    {
        writer.WriteStartElement("url");
        writer.WriteElementString("loc", baseUrl + path);
        writer.WriteElementString("changefreq", changeFreq);
        writer.WriteElementString("priority", priority);
        writer.WriteElementString("lastmod", DateTime.UtcNow.ToString("yyyy-MM-dd"));

        // hreflang alternate links
        foreach (var code in cultures)
        {
            writer.WriteStartElement("xhtml", "link", null);
            writer.WriteAttributeString("rel", "alternate");
            writer.WriteAttributeString("hreflang", code);
            writer.WriteAttributeString("href", $"{baseUrl}{path}?culture={code}");
            writer.WriteEndElement();
        }

        // x-default
        writer.WriteStartElement("xhtml", "link", null);
        writer.WriteAttributeString("rel", "alternate");
        writer.WriteAttributeString("hreflang", "x-default");
        writer.WriteAttributeString("href", $"{baseUrl}{path}?culture=en");
        writer.WriteEndElement();

        writer.WriteEndElement(); // url
    }
}
