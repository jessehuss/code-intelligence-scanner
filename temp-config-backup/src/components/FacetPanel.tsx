'use client'

import { useState } from 'react'
import { SearchFacets, SearchFilters } from '@/lib/types'
import { cn } from '@/lib/utils'
import { ChevronDown, ChevronRight, X } from 'lucide-react'

interface FacetPanelProps {
  facets: SearchFacets
  selectedFacets: SearchFilters
  onFacetChange: (facets: SearchFilters) => void
  className?: string
}

export function FacetPanel({
  facets,
  selectedFacets,
  onFacetChange,
  className
}: FacetPanelProps) {
  const [expandedFacets, setExpandedFacets] = useState<Record<string, boolean>>({
    repository: true,
    service: true,
    operation: true,
    changedSince: true
  })

  const toggleFacet = (facetName: string) => {
    setExpandedFacets(prev => ({
      ...prev,
      [facetName]: !prev[facetName]
    }))
  }

  const toggleFacetValue = (facetName: string, value: string) => {
    const currentValues = selectedFacets[facetName as keyof SearchFilters] as string[] || []
    const newValues = currentValues.includes(value)
      ? currentValues.filter(v => v !== value)
      : [...currentValues, value]

    onFacetChange({
      ...selectedFacets,
      [facetName]: newValues.length > 0 ? newValues : undefined
    })
  }

  const clearFacet = (facetName: string) => {
    onFacetChange({
      ...selectedFacets,
      [facetName]: undefined
    })
  }

  const clearAllFacets = () => {
    onFacetChange({})
  }

  const hasActiveFacets = Object.values(selectedFacets).some(values => 
    Array.isArray(values) && values.length > 0
  )

  const renderFacet = (facetName: string, values: string[]) => {
    const isExpanded = expandedFacets[facetName]
    const selectedValues = selectedFacets[facetName as keyof SearchFilters] as string[] || []
    const hasSelection = selectedValues.length > 0

    return (
      <div key={facetName} className="border-b border-border pb-4 last:border-b-0">
        <div className="flex items-center justify-between">
          <button
            onClick={() => toggleFacet(facetName)}
            className="flex items-center gap-2 text-sm font-medium text-foreground hover:text-primary"
          >
            {isExpanded ? (
              <ChevronDown className="h-4 w-4" />
            ) : (
              <ChevronRight className="h-4 w-4" />
            )}
            <span className="capitalize">{facetName}</span>
            {hasSelection && (
              <span className="ml-1 rounded-full bg-primary px-2 py-0.5 text-xs text-primary-foreground">
                {selectedValues.length}
              </span>
            )}
          </button>
          
          {hasSelection && (
            <button
              onClick={() => clearFacet(facetName)}
              className="text-xs text-muted-foreground hover:text-foreground"
              aria-label={`Clear ${facetName} filters`}
            >
              <X className="h-3 w-3" />
            </button>
          )}
        </div>

        {isExpanded && (
          <div className="mt-2 space-y-1">
            {values.map((value) => {
              const isSelected = selectedValues.includes(value)
              return (
                <label
                  key={value}
                  className="flex items-center gap-2 text-sm cursor-pointer hover:text-primary"
                >
                  <input
                    type="checkbox"
                    checked={isSelected}
                    onChange={() => toggleFacetValue(facetName, value)}
                    className="h-4 w-4 rounded border-border text-primary focus:ring-2 focus:ring-ring focus:ring-offset-2"
                    data-testid={`facet-${facetName}-${value.toLowerCase().replace(/\s+/g, '-')}`}
                  />
                  <span className="flex-1 truncate">{value}</span>
                </label>
              )
            })}
          </div>
        )}
      </div>
    )
  }

  return (
    <div className={cn('w-full max-w-xs space-y-4', className)}>
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-foreground">Filters</h3>
        {hasActiveFacets && (
          <button
            onClick={clearAllFacets}
            className="text-xs text-muted-foreground hover:text-foreground"
            data-testid="clear-all-facets"
          >
            Clear all
          </button>
        )}
      </div>

      <div className="space-y-4">
        {Object.entries(facets).map(([facetName, values]) => 
          values.length > 0 ? renderFacet(facetName, values) : null
        )}
      </div>

      {hasActiveFacets && (
        <div className="pt-4 border-t border-border">
          <div className="space-y-2">
            <h4 className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
              Active Filters
            </h4>
            {Object.entries(selectedFacets).map(([facetName, values]) => {
              if (!Array.isArray(values) || values.length === 0) return null
              
              return (
                <div key={facetName} className="space-y-1">
                  <span className="text-xs text-muted-foreground capitalize">
                    {facetName}:
                  </span>
                  <div className="flex flex-wrap gap-1">
                    {values.map((value) => (
                      <span
                        key={value}
                        className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-1 text-xs text-primary"
                      >
                        {value}
                        <button
                          onClick={() => toggleFacetValue(facetName, value)}
                          className="hover:text-primary-foreground"
                          aria-label={`Remove ${value} filter`}
                        >
                          <X className="h-3 w-3" />
                        </button>
                      </span>
                    ))}
                  </div>
                </div>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
