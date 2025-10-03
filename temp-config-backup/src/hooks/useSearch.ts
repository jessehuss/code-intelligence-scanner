'use client'

import { useState, useCallback, useEffect } from 'react'
import { SearchResponse, SearchFilters } from '@/lib/types'
import { apiClient } from '@/lib/api'
import { debounce } from '@/lib/utils'

interface UseSearchOptions {
  debounceMs?: number
  initialQuery?: string
  initialFilters?: SearchFilters
}

interface UseSearchReturn {
  query: string
  setQuery: (query: string) => void
  filters: SearchFilters
  setFilters: (filters: SearchFilters) => void
  results: SearchResponse | null
  isLoading: boolean
  error: string | null
  search: (query: string, filters?: SearchFilters) => void
  clearSearch: () => void
}

export function useSearch({
  debounceMs = 300,
  initialQuery = '',
  initialFilters = {}
}: UseSearchOptions = {}): UseSearchReturn {
  const [query, setQuery] = useState(initialQuery)
  const [filters, setFilters] = useState<SearchFilters>(initialFilters)
  const [results, setResults] = useState<SearchResponse | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Debounced search function
  const debouncedSearch = useCallback(
    debounce(async (searchQuery: string, searchFilters: SearchFilters) => {
      if (!searchQuery.trim()) {
        setResults(null)
        setIsLoading(false)
        return
      }

      setIsLoading(true)
      setError(null)

      try {
        const response = await apiClient.search(searchQuery, {
          facets: searchFilters
        })
        setResults(response)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Search failed')
        setResults(null)
      } finally {
        setIsLoading(false)
      }
    }, debounceMs),
    [debounceMs]
  )

  // Search function
  const search = useCallback((searchQuery: string, searchFilters?: SearchFilters) => {
    const filtersToUse = searchFilters || filters
    setQuery(searchQuery)
    setFilters(filtersToUse)
    debouncedSearch(searchQuery, filtersToUse)
  }, [filters, debouncedSearch])

  // Clear search
  const clearSearch = useCallback(() => {
    setQuery('')
    setFilters({})
    setResults(null)
    setError(null)
    setIsLoading(false)
  }, [])

  // Update filters and re-search
  const updateFilters = useCallback((newFilters: SearchFilters) => {
    setFilters(newFilters)
    if (query.trim()) {
      debouncedSearch(query, newFilters)
    }
  }, [query, debouncedSearch])

  // Initial search if query is provided
  useEffect(() => {
    if (initialQuery.trim()) {
      search(initialQuery, initialFilters)
    }
  }, []) // Only run on mount

  return {
    query,
    setQuery,
    filters,
    setFilters: updateFilters,
    results,
    isLoading,
    error,
    search,
    clearSearch
  }
}
