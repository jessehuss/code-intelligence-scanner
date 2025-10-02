namespace CatalogApi.Models.DTOs
{
    public class GraphResponse
    {
        public List<GraphNode> Nodes { get; set; } = new();
        public List<GraphEdge> Edges { get; set; } = new();
        public TimeSpan QueryTime { get; set; }
        public int TotalNodes { get; set; }
        public int TotalEdges { get; set; }
    }
}
