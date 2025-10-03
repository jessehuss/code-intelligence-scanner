using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CatalogExplorer.Tests.Contract
{
    public class GraphContractTests
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public GraphContractTests()
        {
            _httpClient = new HttpClient();
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8080/api";
        }

        [Fact]
        public async Task GetGraph_WithDefaultParameters_ReturnsGraphData()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Graph API returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            
            // Verify response structure
            Assert.Contains("nodes", content);
            Assert.Contains("edges", content);
            Assert.Contains("metadata", content);
        }

        [Fact]
        public async Task GetGraph_WithEdgeKindFilter_ReturnsFilteredGraph()
        {
            // Arrange
            var edgeKind = "USES";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph?edgeKind={edgeKind}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Graph API with edge filter returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("nodes", content);
            Assert.Contains("edges", content);
        }

        [Fact]
        public async Task GetGraph_WithDepthLimit_ReturnsLimitedGraph()
        {
            // Arrange
            var depth = 2;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph?depth={depth}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Graph API with depth limit returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("nodes", content);
            Assert.Contains("edges", content);
        }

        [Fact]
        public async Task GetGraph_WithFocusNode_ReturnsFocusedGraph()
        {
            // Arrange
            var focusNode = "User";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph?focus={focusNode}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Graph API with focus node returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("nodes", content);
            Assert.Contains("edges", content);
        }

        [Fact]
        public async Task GetGraph_WithMultipleFilters_ReturnsFilteredGraph()
        {
            // Arrange
            var edgeKind = "USES";
            var depth = 2;
            var focusNode = "User";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph?edgeKind={edgeKind}&depth={depth}&focus={focusNode}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Graph API with multiple filters returned {response.StatusCode}");
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            Assert.Contains("nodes", content);
            Assert.Contains("edges", content);
        }

        [Fact]
        public async Task GetGraph_WithInvalidEdgeKind_ReturnsBadRequest()
        {
            // Arrange
            var edgeKind = "INVALID_EDGE_KIND";
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph?edgeKind={edgeKind}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetGraph_WithInvalidDepth_ReturnsBadRequest()
        {
            // Arrange
            var depth = -1;
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph?depth={depth}");

            // Act
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetGraph_ResponseTime_IsUnder500ms()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/graph");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _httpClient.SendAsync(request);
            stopwatch.Stop();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Graph API returned {response.StatusCode}");
            Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Graph request took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
