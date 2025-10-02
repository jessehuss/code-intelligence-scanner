namespace CatalogApi.Models.DTOs
{
    public class SearchResponse
    {
        public List<SearchResult> Results { get; set; } = new();
        public int TotalCount { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public bool HasMore { get; set; }
        public TimeSpan QueryTime { get; set; }
    }
}
