'use client'

import { SearchResult } from '@/lib/types'
import { cn } from '@/lib/utils'
import { Database, Type, Field, Search, Server } from 'lucide-react'

interface KindResultsProps {
  results: SearchResult[]
  kind: 'Collection' | 'Type' | 'Field' | 'Query' | 'Service'
  onItemClick: (item: SearchResult) => void
  className?: string
}

const kindIcons = {
  Collection: Database,
  Type: Type,
  Field: Field,
  Query: Search,
  Service: Server
}

const kindColors = {
  Collection: 'text-blue-600 bg-blue-50 border-blue-200',
  Type: 'text-green-600 bg-green-50 border-green-200',
  Field: 'text-purple-600 bg-purple-50 border-purple-200',
  Query: 'text-orange-600 bg-orange-50 border-orange-200',
  Service: 'text-red-600 bg-red-50 border-red-200'
}

export function KindResults({
  results,
  kind,
  onItemClick,
  className
}: KindResultsProps) {
  const Icon = kindIcons[kind]
  const colorClass = kindColors[kind]

  if (results.length === 0) {
    return null
  }

  return (
    <div className={cn('space-y-2', className)} data-testid={`results-${kind.toLowerCase()}s`}>
      <div className="flex items-center gap-2">
        <div className={cn(
          'flex h-6 w-6 items-center justify-center rounded border',
          colorClass
        )}>
          <Icon className="h-3 w-3" />
        </div>
        <h3 className="text-sm font-medium text-foreground">
          {kind}s ({results.length})
        </h3>
      </div>
      
      <div className="space-y-1">
        {results.map((result) => (
          <button
            key={result.id}
            onClick={() => onItemClick(result)}
            className={cn(
              'w-full rounded-md border border-transparent p-3 text-left transition-colors',
              'hover:border-border hover:bg-muted/50',
              'focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
              'group'
            )}
            data-testid={`result-${kind.toLowerCase()}-${result.name.toLowerCase().replace(/\s+/g, '-')}`}
          >
            <div className="flex items-start gap-3">
              <div className={cn(
                'mt-0.5 flex h-5 w-5 items-center justify-center rounded border',
                colorClass
              )}>
                <Icon className="h-3 w-3" />
              </div>
              
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <span className="font-medium text-foreground group-hover:text-primary">
                    {result.name}
                  </span>
                  {result.metadata?.namespace && (
                    <span className="text-xs text-muted-foreground">
                      {result.metadata.namespace}
                    </span>
                  )}
                </div>
                
                {result.description && (
                  <p className="mt-1 text-sm text-muted-foreground line-clamp-2">
                    {result.description}
                  </p>
                )}
                
                {result.metadata && (
                  <div className="mt-2 flex flex-wrap gap-2">
                    {result.metadata.usageCount && (
                      <span className="inline-flex items-center rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground">
                        {result.metadata.usageCount} usages
                      </span>
                    )}
                    {result.metadata.lastModified && (
                      <span className="inline-flex items-center rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground">
                        {result.metadata.lastModified}
                      </span>
                    )}
                    {result.metadata.repository && (
                      <span className="inline-flex items-center rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground">
                        {result.metadata.repository.split('/').pop()}
                      </span>
                    )}
                  </div>
                )}
              </div>
            </div>
          </button>
        ))}
      </div>
    </div>
  )
}
