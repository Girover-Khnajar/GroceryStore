using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GroceryStore.Api.Contracts.Images;
using GroceryStore.Application.Images.Dtos;

namespace GroceryStore.Api.Tests.Endpoints;

public class ImageEndpointsTests : IClassFixture<GroceryStoreApiFactory>
{
    private readonly HttpClient _client;

    public ImageEndpointsTests(GroceryStoreApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private CreateImageAssetRequest CreateValidImageRequest(string fileName = "test.jpg") =>
        new(
            StoragePath: $"/images/{fileName}",
            Url: $"https://cdn.example.com/images/{fileName}",
            FileName: fileName,
            ContentType: "image/jpeg",
            FileSizeBytes: 102400,
            Width: 800,
            Height: 600,
            AltText: "A test image");

    private async Task<Guid> CreateImageAsync(string fileName = "test.jpg")
    {
        var request = CreateValidImageRequest(fileName);
        var response = await _client.PostAsJsonAsync("/api/images", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<IdResponse>();
        return result!.Id;
    }

    #region GET /api/images/{id}

    [Fact]
    public async Task GetImageById_WhenImageExists_ReturnsImage()
    {
        // Arrange
        var imageId = await CreateImageAsync("getbyid.jpg");

        // Act
        var response = await _client.GetAsync($"/api/images/{imageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var image = await response.Content.ReadFromJsonAsync<ImageAssetDto>();
        image.Should().NotBeNull();
        image!.OriginalFileName.Should().Be("getbyid.jpg");
        image.ContentType.Should().Be("image/jpeg");
        image.AltText.Should().Be("A test image");
    }

    [Fact]
    public async Task GetImageById_WhenImageDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/images/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/images/upload

    [Fact]
    public async Task UploadImage_WithValidPng_ReturnsCreated_AndCanBeFetched()
    {
        // Arrange
        // 1x1 transparent PNG
        var pngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO2Z2z8AAAAASUVORK5CYII=");

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        form.Add(fileContent, "File", "tiny.png");
        form.Add(new StringContent("Tiny image"), "AltText");

        // Act
        var response = await _client.PostAsync("/api/images/upload", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<IdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();

        var getResponse = await _client.GetAsync($"/api/images/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var image = await getResponse.Content.ReadFromJsonAsync<ImageAssetDto>();
        image.Should().NotBeNull();
        image!.OriginalFileName.Should().Be("tiny.png");
        image.ContentType.Should().Be("image/png");
        image.AltText.Should().Be("Tiny image");
        image.StoragePath.Should().StartWith("/images/uploads/");
    }

    #endregion

    #region GET /api/images/batch

    [Fact]
    public async Task GetImagesByIds_WhenImagesExist_ReturnsImages()
    {
        // Arrange
        var imageId1 = await CreateImageAsync("batch1.jpg");
        var imageId2 = await CreateImageAsync("batch2.jpg");

        // Act
        var response = await _client.GetAsync(
            $"/api/images/batch?ids={imageId1}&ids={imageId2}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var images = await response.Content.ReadFromJsonAsync<List<ImageAssetDto>>();
        images.Should().NotBeNull();
        images.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetImagesByIds_WhenNoIdsProvided_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/images/batch");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var images = await response.Content.ReadFromJsonAsync<List<ImageAssetDto>>();
        images.Should().NotBeNull();
        images.Should().BeEmpty();
    }

    #endregion

    #region POST /api/images

    [Fact]
    public async Task CreateImage_WithValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = CreateValidImageRequest("created.jpg");

        // Act
        var response = await _client.PostAsJsonAsync("/api/images", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<IdResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateImage_WithAllFields_ReturnsCreated()
    {
        // Arrange
        var request = new CreateImageAssetRequest(
            StoragePath: "/images/full.png",
            Url: "https://cdn.example.com/images/full.png",
            FileName: "full.png",
            ContentType: "image/png",
            FileSizeBytes: 204800,
            Width: 1920,
            Height: 1080,
            AltText: "Full image");

        // Act
        var response = await _client.PostAsJsonAsync("/api/images", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateImage_WithoutAltText_ReturnsCreated()
    {
        // Arrange
        var request = new CreateImageAssetRequest(
            StoragePath: "/images/noalt.jpg",
            Url: "https://cdn.example.com/images/noalt.jpg",
            FileName: "noalt.jpg",
            ContentType: "image/jpeg",
            FileSizeBytes: 50000,
            Width: 640,
            Height: 480);

        // Act
        var response = await _client.PostAsJsonAsync("/api/images", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region PUT /api/images/{id}/alt-text

    [Fact]
    public async Task UpdateImageAltText_WhenImageExists_ReturnsNoContent()
    {
        // Arrange
        var imageId = await CreateImageAsync("updatealt.jpg");
        var updateRequest = new UpdateImageAltTextRequest("Updated alt text");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/images/{imageId}/alt-text", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/images/{imageId}");
        var image = await getResponse.Content.ReadFromJsonAsync<ImageAssetDto>();
        image!.AltText.Should().Be("Updated alt text");
    }

    [Fact]
    public async Task UpdateImageAltText_WithNullAltText_ReturnsNoContent()
    {
        // Arrange
        var imageId = await CreateImageAsync("clearalt.jpg");
        var updateRequest = new UpdateImageAltTextRequest(null);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/images/{imageId}/alt-text", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateImageAltText_WhenImageDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateImageAltTextRequest("Alt text");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/images/{Guid.NewGuid()}/alt-text", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/images/{id}

    [Fact]
    public async Task DeleteImage_WhenImageExists_ReturnsNoContent()
    {
        // Arrange
        var imageId = await CreateImageAsync("deleteme.jpg");

        // Act
        var response = await _client.DeleteAsync($"/api/images/{imageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteImage_WhenImageDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/images/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    private sealed record IdResponse(Guid Id);
}
