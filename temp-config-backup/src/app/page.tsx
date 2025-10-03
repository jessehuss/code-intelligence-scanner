import Link from 'next/link'
import { Search, Network, Database, FileText, ArrowRight } from 'lucide-react'

export default function HomePage() {
  return (
    <div className="min-h-screen bg-background">
      {/* Hero Section */}
      <div className="container mx-auto px-4 py-16">
        <div className="text-center max-w-4xl mx-auto">
          <h1 className="text-4xl md:text-6xl font-bold text-foreground mb-6">
            Discover Your Codebase
          </h1>
          <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
            Search-first web app for backend developers and SREs to discover Collections, Types, Fields, Queries, and Services in your codebase.
          </p>
          
          {/* Search Bar */}
          <div className="max-w-2xl mx-auto mb-12">
            <div className="relative">
              <input
                type="text"
                placeholder="Search collections, types, fields, queries..."
                className="w-full px-6 py-4 pl-12 text-lg border border-border rounded-lg bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent shadow-lg"
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    const query = (e.target as HTMLInputElement).value
                    if (query.trim()) {
                      window.location.href = `/search?q=${encodeURIComponent(query.trim())}`
                    }
                  }
                }}
              />
              <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 h-6 w-6 text-muted-foreground" />
            </div>
          </div>

          {/* Quick Actions */}
          <div className="flex flex-wrap justify-center gap-4">
            <Link
              href="/search"
              className="inline-flex items-center gap-2 px-6 py-3 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors"
            >
              <Search className="h-5 w-5" />
              Start Searching
            </Link>
            <Link
              href="/graph"
              className="inline-flex items-center gap-2 px-6 py-3 border border-border text-foreground rounded-lg hover:bg-muted transition-colors"
            >
              <Network className="h-5 w-5" />
              View Graph
            </Link>
          </div>
        </div>
      </div>

      {/* Features Section */}
      <div className="container mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-foreground mb-4">Features</h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            Powerful tools to explore and understand your codebase structure and relationships.
          </p>
        </div>

        <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-4">
          {/* Global Search */}
          <div className="text-center p-6 border border-border rounded-lg hover:bg-muted/50 transition-colors">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-primary/10 text-primary rounded-lg mb-4">
              <Search className="h-6 w-6" />
            </div>
            <h3 className="text-lg font-semibold text-foreground mb-2">Global Search</h3>
            <p className="text-muted-foreground mb-4">
              Instant search across Collections, Types, Fields, Queries, and Services with federated results.
            </p>
            <Link
              href="/search"
              className="inline-flex items-center gap-1 text-sm text-primary hover:text-primary/80 transition-colors"
            >
              Try Search <ArrowRight className="h-3 w-3" />
            </Link>
          </div>

          {/* Graph Visualization */}
          <div className="text-center p-6 border border-border rounded-lg hover:bg-muted/50 transition-colors">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-primary/10 text-primary rounded-lg mb-4">
              <Network className="h-6 w-6" />
            </div>
            <h3 className="text-lg font-semibold text-foreground mb-2">Graph Visualization</h3>
            <p className="text-muted-foreground mb-4">
              Interactive force graph showing relationships between entities with filtering capabilities.
            </p>
            <Link
              href="/graph"
              className="inline-flex items-center gap-1 text-sm text-primary hover:text-primary/80 transition-colors"
            >
              View Graph <ArrowRight className="h-3 w-3" />
            </Link>
          </div>

          {/* Collection Details */}
          <div className="text-center p-6 border border-border rounded-lg hover:bg-muted/50 transition-colors">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-primary/10 text-primary rounded-lg mb-4">
              <Database className="h-6 w-6" />
            </div>
            <h3 className="text-lg font-semibold text-foreground mb-2">Collection Details</h3>
            <p className="text-muted-foreground mb-4">
              Comprehensive view of collection schemas with drift indicators and usage statistics.
            </p>
            <Link
              href="/search?kind=collection"
              className="inline-flex items-center gap-1 text-sm text-primary hover:text-primary/80 transition-colors"
            >
              Browse Collections <ArrowRight className="h-3 w-3" />
            </Link>
          </div>

          {/* Type Information */}
          <div className="text-center p-6 border border-border rounded-lg hover:bg-muted/50 transition-colors">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-primary/10 text-primary rounded-lg mb-4">
              <FileText className="h-6 w-6" />
            </div>
            <h3 className="text-lg font-semibold text-foreground mb-2">Type Information</h3>
            <p className="text-muted-foreground mb-4">
              Detailed type definitions with field information, usage tracking, and relationship mapping.
            </p>
            <Link
              href="/search?kind=type"
              className="inline-flex items-center gap-1 text-sm text-primary hover:text-primary/80 transition-colors"
            >
              Browse Types <ArrowRight className="h-3 w-3" />
            </Link>
          </div>
        </div>
      </div>

      {/* Stats Section */}
      <div className="container mx-auto px-4 py-16">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-foreground mb-4">System Overview</h2>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            Get insights into your codebase structure and usage patterns.
          </p>
        </div>

        <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-4">
          <div className="text-center p-6 border border-border rounded-lg">
            <div className="text-3xl font-bold text-primary mb-2">10,000+</div>
            <div className="text-muted-foreground">Total Entities</div>
          </div>
          <div className="text-center p-6 border border-border rounded-lg">
            <div className="text-3xl font-bold text-primary mb-2">&lt;300ms</div>
            <div className="text-muted-foreground">Search Response</div>
          </div>
          <div className="text-center p-6 border border-border rounded-lg">
            <div className="text-3xl font-bold text-primary mb-2">&lt;400ms</div>
            <div className="text-muted-foreground">Page Load Time</div>
          </div>
          <div className="text-center p-6 border border-border rounded-lg">
            <div className="text-3xl font-bold text-primary mb-2">50+</div>
            <div className="text-muted-foreground">Concurrent Users</div>
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="container mx-auto px-4 py-16">
        <div className="text-center max-w-2xl mx-auto">
          <h2 className="text-3xl font-bold text-foreground mb-4">Ready to Explore?</h2>
          <p className="text-lg text-muted-foreground mb-8">
            Start discovering your codebase structure and relationships today.
          </p>
          <Link
            href="/search"
            className="inline-flex items-center gap-2 px-8 py-4 bg-primary text-primary-foreground rounded-lg hover:bg-primary/90 transition-colors text-lg"
          >
            <Search className="h-5 w-5" />
            Start Searching
          </Link>
        </div>
      </div>
    </div>
  )
}
