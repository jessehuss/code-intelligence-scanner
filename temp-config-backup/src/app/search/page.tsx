'use client'

import { useState, useEffect } from 'react'
import { useSearchParams, useRouter } from 'next/navigation'
import { SearchBar } from '@/components/SearchBar'
import { KindResults } from '@/components/KindResults'
import { FacetPanel } from '@/components/FacetPanel'
import { useSearch } from '@/hooks/useSearch'
import { useCommonShortcuts } from '@/hooks/useKeyboard'
import { SearchResult, SearchFilters } from '@/lib/types'
import { parseSearchUrl, buildSearchUrl } from '@/lib/utils'
import { Search, AlertCircle, Loader2 } from 'lucide-react'

export default function SearchPage() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const [selectedFacets, setSelectedFacets] = useState<SearchFilters>({})

  // Parse URL parameters
  const { query: urlQuery, filters: urlFilters } = parseSearchUrl(searchParams)

  // Initialize search hook
  const {
    query,
    setQuery,
    filters,
    setFilters,
    results,
    isLoading,
    error,
    search,
    clearSearch
  } = useSearch({
    initialQuery: urlQuery,
    initialFilters: urlFilters
  })

  // Update URL when search changes
  useEffect(() => {
    if (query || Object.keys(filters).length > 0) {
      const newUrl = buildSearchUrl(query, filters)
      router.replace(newUrl, { scroll: false })
    }
  }, [query, filters, router])

  // Handle search input
  const handleSearch = (searchQuery: string) => {
    search(searchQuery, selectedFacets)
  }

  // Handle facet changes
  const handleFacetChange = (newFacets: SearchFilters) => {
    setSelectedFacets(newFacets)
    setFilters(newFacets)
    if (query.trim()) {
      search(query, newFacets)
    }
  }

  // Handle result item clicks
  const handleItemClick = (item: SearchResult) => {
    switch (item.kind) {
      case 'Collection':
        router.push(`/collections/${item.name}`)
        break
      case 'Type':
        router.push(`/types/${item.id}`)
        break
      case 'Field':
        // Navigate to the parent type
        if (item.metadata?.parentType) {
          router.push(`/types/${item.metadata.parentType}`)
        }
        break
      case 'Query':
        // Navigate to the collection that uses this query
        if (item.metadata?.collection) {
          router.push(`/collections/${item.metadata.collection}`)
        }
        break
      case 'Service':
        // Navigate to service detail (if implemented)
        router.push(`/services/${item.name}`)
        break
    }
  }

  // Group results by kind
  const groupedResults = results ? {
    Collections: results.results.filter(r => r.kind === 'Collection'),
    Types: results.results.filter(r => r.kind === 'Type'),
    Fields: results.results.filter(r => r.kind === 'Field'),
    Queries: results.results.filter(r => r.kind === 'Query'),
    Services: results.results.filter(r => r.kind === 'Service')
  } : null

  // Enable common keyboard shortcuts
  useCommonShortcuts()

  return (
    <div className="min-h-screen bg-background">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">Search Catalog</h1>
          <p className="text-muted-foreground">
            Discover collections, types, fields, queries, and services across your codebase
          </p>
        </div>

        {/* Search Interface */}
        <div className="flex gap-8">
          {/* Main Content */}
          <div className="flex-1 space-y-6">
            {/* Search Bar */}
            <div className="flex justify-center">
              <SearchBar
                onSearch={handleSearch}
                initialQuery={query}
                className="w-full max-w-2xl"
              />
            </div>

            {/* Loading State */}
            {isLoading && (
              <div className="flex items-center justify-center py-12" data-testid="search-loading">
                <div className="flex items-center gap-2 text-muted-foreground">
                  <Loader2 className="h-5 w-5 animate-spin" />
                  Searching...
                </div>
              </div>
            )}

            {/* Error State */}
            {error && (
              <div className="flex items-center justify-center py-12" data-testid="search-error">
                <div className="text-center">
                  <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
                  <h3 className="text-lg font-medium text-foreground mb-2">Search Failed</h3>
                  <p className="text-muted-foreground mb-4">{error}</p>
                  <button
                    onClick={() => search(query, filters)}
                    className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90 transition-colors"
                    data-testid="retry-search"
                  >
                    <Search className="h-4 w-4" />
                    Try Again
                  </button>
                </div>
              </div>
            )}

            {/* Results */}
            {results && !isLoading && (
              <div className="space-y-6" data-testid="search-results">
                {/* Results Summary */}
                <div className="flex items-center justify-between">
                  <div className="text-sm text-muted-foreground">
                    Found {results.pagination.total} results for "{query}"
                    {results.pagination.total > 0 && (
                      <span className="ml-2">
                        (took {results.executionTime}ms)
                      </span>
                    )}
                  </div>
                  {results.pagination.total > 0 && (
                    <div className="text-sm text-muted-foreground">
                      Page {results.pagination.page} of {results.pagination.totalPages}
                    </div>
                  )}
                </div>

                {/* Empty Results */}
                {results.pagination.total === 0 && (
                  <div className="text-center py-12" data-testid="empty-results">
                    <Search className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                    <h3 className="text-lg font-medium text-foreground mb-2">No results found</h3>
                    <p className="text-muted-foreground">
                      Try adjusting your search terms or filters
                    </p>
                  </div>
                )}

                {/* Grouped Results */}
                {results.pagination.total > 0 && (
                  <div className="space-y-8">
                    {groupedResults && Object.entries(groupedResults).map(([kind, items]) => 
                      items.length > 0 && (
                        <KindResults
                          key={kind}
                          results={items}
                          kind={kind as any}
                          onItemClick={handleItemClick}
                        />
                      )
                    )}
                  </div>
                )}

                {/* Pagination */}
                {results.pagination.totalPages > 1 && (
                  <div className="flex items-center justify-center gap-2">
                    <button
                      onClick={() => {
                        if (results.pagination.hasPrev) {
                          search(query, filters, results.pagination.page - 1)
                        }
                      }}
                      disabled={!results.pagination.hasPrev}
                      className="px-3 py-2 text-sm border border-border rounded-md hover:bg-muted disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Previous
                    </button>
                    
                    <span className="px-3 py-2 text-sm text-muted-foreground">
                      {results.pagination.page} of {results.pagination.totalPages}
                    </span>
                    
                    <button
                      onClick={() => {
                        if (results.pagination.hasNext) {
                          search(query, filters, results.pagination.page + 1)
                        }
                      }}
                      disabled={!results.pagination.hasNext}
                      className="px-3 py-2 text-sm border border-border rounded-md hover:bg-muted disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      Next
                    </button>
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Facets Sidebar */}
          {results && results.facets && (
            <div className="w-80">
              <FacetPanel
                facets={results.facets}
                selectedFacets={filters}
                onFacetChange={handleFacetChange}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
