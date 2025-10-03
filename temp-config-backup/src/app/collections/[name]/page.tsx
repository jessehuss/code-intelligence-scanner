'use client'

import { useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { SchemaTable } from '@/components/SchemaTable'
import { MiniGraph } from '@/components/MiniGraph'
import { CodeSnippet } from '@/components/CodeSnippet'
import { useCollection } from '@/hooks/useApi'
import { buildGitUrl } from '@/lib/utils'
import { Database, ExternalLink, AlertCircle, Loader2, ArrowLeft } from 'lucide-react'

type TabType = 'schema' | 'types' | 'queries' | 'relationships'

export default function CollectionDetailPage() {
  const params = useParams()
  const router = useRouter()
  const collectionName = params.name as string
  const [activeTab, setActiveTab] = useState<TabType>('schema')

  const { data: collection, isLoading, error, retry } = useCollection(collectionName)

  const tabs = [
    { id: 'schema' as const, label: 'Schema', icon: Database },
    { id: 'types' as const, label: 'Types', icon: Database },
    { id: 'queries' as const, label: 'Queries', icon: Database },
    { id: 'relationships' as const, label: 'Relationships', icon: Database }
  ]

  const handleTypeClick = (typeId: string) => {
    router.push(`/types/${typeId}`)
  }

  const handleQueryClick = (queryId: string) => {
    // For now, just show the query in a modal or expand it
    console.log('Query clicked:', queryId)
  }

  const handleNodeClick = (node: any) => {
    if (node.type === 'Collection') {
      router.push(`/collections/${node.id}`)
    } else if (node.type === 'Type') {
      router.push(`/types/${node.id}`)
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center" data-testid="collection-loading">
        <div className="flex items-center gap-2 text-muted-foreground">
          <Loader2 className="h-5 w-5 animate-spin" />
          Loading collection...
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center" data-testid="collection-error">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="text-lg font-medium text-foreground mb-2">Failed to load collection</h3>
          <p className="text-muted-foreground mb-4">{error}</p>
          <button
            onClick={retry}
            className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90 transition-colors"
            data-testid="retry-collection"
          >
            <AlertCircle className="h-4 w-4" />
            Try Again
          </button>
        </div>
      </div>
    )
  }

  if (!collection) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <Database className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-medium text-foreground mb-2">Collection not found</h3>
          <p className="text-muted-foreground mb-4">The collection "{collectionName}" does not exist.</p>
          <button
            onClick={() => router.push('/search')}
            className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90 transition-colors"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Search
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-background">
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <button
            onClick={() => router.back()}
            className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground mb-4"
          >
            <ArrowLeft className="h-4 w-4" />
            Back
          </button>
          
          <div className="flex items-start justify-between">
            <div>
              <h1 className="text-3xl font-bold text-foreground mb-2">{collection.name}</h1>
              <p className="text-muted-foreground">
                Collection with {collection.declaredSchema.length} declared fields and {collection.observedSchema.length} observed fields
              </p>
            </div>
            
            {/* Provenance Link */}
            {collection.provenance && (
              <a
                href={buildGitUrl(collection.provenance)}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex items-center gap-2 px-3 py-2 text-sm border border-border rounded-md hover:bg-muted transition-colors"
                data-testid="provenance-link"
              >
                <ExternalLink className="h-4 w-4" />
                View Source
              </a>
            )}
          </div>
        </div>

        {/* Tabs */}
        <div className="border-b border-border mb-6">
          <nav className="flex space-x-8">
            {tabs.map((tab) => {
              const Icon = tab.icon
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`
                    flex items-center gap-2 py-4 px-1 border-b-2 font-medium text-sm transition-colors
                    ${activeTab === tab.id
                      ? 'border-primary text-primary'
                      : 'border-transparent text-muted-foreground hover:text-foreground hover:border-border'
                    }
                  `}
                  data-testid={`tab-${tab.id}`}
                >
                  <Icon className="h-4 w-4" />
                  {tab.label}
                </button>
              )
            })}
          </nav>
        </div>

        {/* Tab Content */}
        <div data-testid="collection-detail">
          {activeTab === 'schema' && (
            <div>
              <SchemaTable
                declaredSchema={collection.declaredSchema}
                observedSchema={collection.observedSchema}
                presenceMetrics={collection.presenceMetrics}
                driftIndicators={collection.driftIndicators}
              />
            </div>
          )}

          {activeTab === 'types' && (
            <div data-testid="types-list">
              <div className="space-y-4">
                <h3 className="text-lg font-medium text-foreground">Types Used</h3>
                {collection.types.length > 0 ? (
                  <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                    {collection.types.map((type) => (
                      <button
                        key={type.id}
                        onClick={() => handleTypeClick(type.id)}
                        className="p-4 border border-border rounded-lg hover:bg-muted transition-colors text-left"
                        data-testid="type-item"
                      >
                        <div className="font-medium text-foreground">{type.name}</div>
                        <div className="text-sm text-muted-foreground mt-1">{type.namespace}</div>
                        <div className="text-xs text-muted-foreground mt-2">
                          {type.usageCount} usages
                        </div>
                      </button>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted-foreground">No types found for this collection.</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'queries' && (
            <div data-testid="queries-list">
              <div className="space-y-4">
                <h3 className="text-lg font-medium text-foreground">Queries</h3>
                {collection.queries.length > 0 ? (
                  <div className="space-y-4">
                    {collection.queries.map((query) => (
                      <div
                        key={query.id}
                        className="border border-border rounded-lg p-4"
                        data-testid="query-item"
                      >
                        <div className="flex items-center justify-between mb-3">
                          <div>
                            <h4 className="font-medium text-foreground">{query.name}</h4>
                            <p className="text-sm text-muted-foreground">
                              {query.operation} • {query.usageCount} usages
                            </p>
                          </div>
                          <button
                            onClick={() => handleQueryClick(query.id)}
                            className="text-sm text-primary hover:text-primary/80"
                          >
                            View Details
                          </button>
                        </div>
                        
                        {/* Placeholder for query code snippet */}
                        <div className="bg-muted/50 rounded p-3">
                          <CodeSnippet
                            code={`// ${query.operation} query for ${collection.name}\n// Implementation details would be shown here`}
                            language="javascript"
                            height={80}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted-foreground">No queries found for this collection.</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'relationships' && (
            <div data-testid="relationships-section">
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <h3 className="text-lg font-medium text-foreground">Relationships</h3>
                  <button
                    onClick={() => router.push('/graph')}
                    className="text-sm text-primary hover:text-primary/80"
                    data-testid="view-full-graph"
                  >
                    View Full Graph
                  </button>
                </div>
                
                {collection.relationships.length > 0 ? (
                  <MiniGraph
                    nodes={[
                      { id: collection.id, label: collection.name, type: 'Collection' },
                      ...collection.relationships.map(rel => ({
                        id: rel.targetId,
                        label: rel.targetId,
                        type: 'Type' as const
                      }))
                    ]}
                    edges={collection.relationships.map(rel => ({
                      id: rel.id,
                      source: rel.sourceId,
                      target: rel.targetId,
                      edgeKind: rel.edgeKind,
                      weight: rel.weight
                    }))}
                    onNodeClick={handleNodeClick}
                    height={400}
                    data-testid="mini-graph"
                  />
                ) : (
                  <p className="text-muted-foreground">No relationships found for this collection.</p>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
