using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CatalogExplorer.Tests.Contract
{
    public class CollectionContractTests
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public CollectionContractTests()
        {
            _httpClient = new HttpClient();
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8080/api";
        }

        [Fact]
        public async Task GetCollection_WithValidName_ReturnsCollectionDetails()
        {
            // Arrange
            var collectionName = "users";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/collections/{collectionName}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Collection API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            
            // Verify response structure
            Assert.Contains("id", content);
            Assert.Contains("name", content);
            Assert.Contains("declaredSchema", content);
            Assert.Contains("observedSchema", content);
            Assert.Contains("presenceMetrics", content);
            Assert.Contains("driftIndicators", content);
            Assert.Contains("types", content);
            Assert.Contains("queries", content);
            Assert.Contains("relationships", content);
            Assert.Contains("provenance", content);
        }

        [Fact]
        public async Task GetCollection_WithInvalidName_ReturnsNotFound()
        {
            // Arrange
            var collectionName = "nonexistent-collection";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/collections/{collectionName}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetCollectionSchema_WithValidName_ReturnsSchemaDetails()
        {
            // Arrange
            var collectionName = "users";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/collections/{collectionName}/schema");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Collection schema API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("declaredSchema", content);
            Assert.Contains("observedSchema", content);
            Assert.Contains("driftIndicators", content);
        }

        [Fact]
        public async Task GetCollectionTypes_WithValidName_ReturnsTypesList()
        {
            // Arrange
            var collectionName = "users";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/collections/{collectionName}/types");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Collection types API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("types", content);
        }

        [Fact]
        public async Task GetCollectionQueries_WithValidName_ReturnsQueriesList()
        {
            // Arrange
            var collectionName = "users";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/collections/{collectionName}/queries");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Collection queries API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("queries", content);
        }

        [Fact]
        public async Task GetCollectionRelationships_WithValidName_ReturnsRelationships()
        {
            // Arrange
            var collectionName = "users";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/collections/{collectionName}/relationships");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Collection relationships API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("relationships", content);
        }

        [Fact]
        public async Task GetCollection_ResponseTime_IsUnder400ms()
        {
            // Arrange
            var collectionName = "users";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/collections/{collectionName}");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Collection API returned {response.StatusCode}");
            Assert.True(stopwatch.ElapsedMilliseconds < 400, $"Collection request took {stopwatch.ElapsedMilliseconds}ms, expected < 400ms");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
