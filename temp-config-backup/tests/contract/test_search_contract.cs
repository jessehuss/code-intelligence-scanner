using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CatalogExplorer.Tests.Contract
{
    public class SearchContractTests
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SearchContractTests()
        {
            _httpClient = new HttpClient();
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8080/api";
        }

        [Fact]
        public async Task Search_WithValidQuery_ReturnsSearchResults()
        {
            // Arrange
            var query = "user";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/search?q={query}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Search API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            
            // Verify response structure
            Assert.Contains("results", content);
            Assert.Contains("facets", content);
            Assert.Contains("pagination", content);
        }

        [Fact]
        public async Task Search_WithEmptyQuery_ReturnsBadRequest()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/search?q=");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Search_WithFacets_ReturnsFilteredResults()
        {
            // Arrange
            var query = "user";
            var facets = "repository:main,service:api";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/search?q={query}&facets={facets}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Search API with facets returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task Search_WithPagination_ReturnsPaginatedResults()
        {
            // Arrange
            var query = "user";
            var page = 1;
            var limit = 10;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/search?q={query}&page={page}&limit={limit}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Search API with pagination returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("pagination", content);
        }

        [Fact]
        public async Task Search_ResponseTime_IsUnder300ms()
        {
            // Arrange
            var query = "user";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/search?q={query}");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Search API returned {response.StatusCode}");
            Assert.True(stopwatch.ElapsedMilliseconds < 300, $"Search took {stopwatch.ElapsedMilliseconds}ms, expected < 300ms");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
