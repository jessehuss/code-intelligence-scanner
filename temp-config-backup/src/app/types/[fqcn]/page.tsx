'use client'

import { useState } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { SchemaTable } from '@/components/SchemaTable'
import { MiniGraph } from '@/components/MiniGraph'
import { CodeSnippet } from '@/components/CodeSnippet'
import { useType } from '@/hooks/useApi'
import { buildGitUrl } from '@/lib/utils'
import { FileText, ExternalLink, AlertCircle, Loader2, ArrowLeft, GitBranch } from 'lucide-react'

type TabType = 'overview' | 'fields' | 'usages' | 'relationships' | 'diff'

export default function TypeDetailPage() {
  const params = useParams()
  const router = useRouter()
  const typeFqcn = params.fqcn as string
  const [activeTab, setActiveTab] = useState<TabType>('overview')

  const { data: type, isLoading, error, retry } = useType(typeFqcn)

  const tabs = [
    { id: 'overview' as const, label: 'Overview', icon: FileText },
    { id: 'fields' as const, label: 'Fields', icon: FileText },
    { id: 'usages' as const, label: 'Usages', icon: GitBranch },
    { id: 'relationships' as const, label: 'Relationships', icon: GitBranch },
    { id: 'diff' as const, label: 'Diff', icon: GitBranch }
  ]

  const handleCollectionClick = (collectionId: string) => {
    router.push(`/collections/${collectionId}`)
  }

  const handleTypeClick = (typeId: string) => {
    router.push(`/types/${typeId}`)
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
      <div className="min-h-screen bg-background flex items-center justify-center" data-testid="type-loading">
        <div className="flex items-center gap-2 text-muted-foreground">
          <Loader2 className="h-5 w-5 animate-spin" />
          Loading type...
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center" data-testid="type-error">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="text-lg font-medium text-foreground mb-2">Failed to load type</h3>
          <p className="text-muted-foreground mb-4">{error}</p>
          <button
            onClick={retry}
            className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90 transition-colors"
            data-testid="retry-type"
          >
            <AlertCircle className="h-4 w-4" />
            Try Again
          </button>
        </div>
      </div>
    )
  }

  if (!type) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-medium text-foreground mb-2">Type not found</h3>
          <p className="text-muted-foreground mb-4">The type "{typeFqcn}" does not exist.</p>
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
              <h1 className="text-3xl font-bold text-foreground mb-2">{type.name}</h1>
              <p className="text-muted-foreground">
                {type.namespace} • {type.usageCount} usages
              </p>
            </div>
            
            {/* Provenance Link */}
            {type.provenance && (
              <a
                href={buildGitUrl(type.provenance)}
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
        <div data-testid="type-detail">
          {activeTab === 'overview' && (
            <div>
              <div className="grid gap-6 md:grid-cols-2">
                {/* Type Information */}
                <div className="space-y-4">
                  <h3 className="text-lg font-medium text-foreground">Type Information</h3>
                  <div className="space-y-3">
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Name</label>
                      <p className="text-foreground">{type.name}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Namespace</label>
                      <p className="text-foreground">{type.namespace}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Usage Count</label>
                      <p className="text-foreground">{type.usageCount}</p>
                    </div>
                    <div>
                      <label className="text-sm font-medium text-muted-foreground">Fields</label>
                      <p className="text-foreground">{type.fields.length} fields</p>
                    </div>
                  </div>
                </div>

                {/* Recent Changes */}
                <div className="space-y-4">
                  <h3 className="text-lg font-medium text-foreground">Recent Changes</h3>
                  {type.diffSummary && (
                    <div className="space-y-2">
                      <div className="text-sm">
                        <span className="text-muted-foreground">Last modified:</span>{' '}
                        <span className="text-foreground">
                          {new Date(type.diffSummary.lastModified).toLocaleDateString()}
                        </span>
                      </div>
                      <div className="text-sm">
                        <span className="text-muted-foreground">Changes:</span>{' '}
                        <span className="text-foreground">
                          {type.diffSummary.addedFields} added, {type.diffSummary.removedFields} removed, {type.diffSummary.modifiedFields} modified
                        </span>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}

          {activeTab === 'fields' && (
            <div data-testid="fields-list">
              <div className="space-y-4">
                <h3 className="text-lg font-medium text-foreground">Fields</h3>
                {type.fields.length > 0 ? (
                  <div className="space-y-3">
                    {type.fields.map((field) => (
                      <div
                        key={field.name}
                        className="border border-border rounded-lg p-4"
                        data-testid="field-item"
                      >
                        <div className="flex items-center justify-between mb-2">
                          <div>
                            <h4 className="font-medium text-foreground">{field.name}</h4>
                            <p className="text-sm text-muted-foreground">{field.type}</p>
                          </div>
                          {field.attributes && field.attributes.length > 0 && (
                            <div className="flex gap-1">
                              {field.attributes.map((attr) => (
                                <span
                                  key={attr.name}
                                  className="px-2 py-1 text-xs bg-muted text-muted-foreground rounded"
                                >
                                  {attr.name}
                                </span>
                              ))}
                            </div>
                          )}
                        </div>
                        
                        {field.documentation && (
                          <p className="text-sm text-muted-foreground">{field.documentation}</p>
                        )}
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted-foreground">No fields found for this type.</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'usages' && (
            <div data-testid="usages-list">
              <div className="space-y-4">
                <h3 className="text-lg font-medium text-foreground">Usages</h3>
                {type.usages.length > 0 ? (
                  <div className="space-y-3">
                    {type.usages.map((usage) => (
                      <div
                        key={usage.id}
                        className="border border-border rounded-lg p-4"
                        data-testid="usage-item"
                      >
                        <div className="flex items-center justify-between mb-2">
                          <div>
                            <h4 className="font-medium text-foreground">{usage.context}</h4>
                            <p className="text-sm text-muted-foreground">{usage.operation}</p>
                          </div>
                          <span className="text-sm text-muted-foreground">
                            {usage.usageCount} usages
                          </span>
                        </div>
                        
                        {usage.provenance && (
                          <a
                            href={buildGitUrl(usage.provenance)}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="inline-flex items-center gap-1 text-sm text-primary hover:text-primary/80"
                          >
                            <ExternalLink className="h-3 w-3" />
                            View Source
                          </a>
                        )}
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted-foreground">No usages found for this type.</p>
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
                
                {type.relationships.length > 0 ? (
                  <MiniGraph
                    nodes={[
                      { id: type.id, label: type.name, type: 'Type' },
                      ...type.relationships.map(rel => ({
                        id: rel.targetId,
                        label: rel.targetId,
                        type: 'Type' as const
                      }))
                    ]}
                    edges={type.relationships.map(rel => ({
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
                  <p className="text-muted-foreground">No relationships found for this type.</p>
                )}
              </div>
            </div>
          )}

          {activeTab === 'diff' && (
            <div data-testid="diff-section">
              <div className="space-y-4">
                <h3 className="text-lg font-medium text-foreground">Schema Diff</h3>
                {type.diffSummary ? (
                  <div className="space-y-4">
                    <div className="grid gap-4 md:grid-cols-3">
                      <div className="border border-border rounded-lg p-4">
                        <h4 className="font-medium text-foreground mb-2">Added Fields</h4>
                        <p className="text-2xl font-bold text-green-600">{type.diffSummary.addedFields}</p>
                      </div>
                      <div className="border border-border rounded-lg p-4">
                        <h4 className="font-medium text-foreground mb-2">Removed Fields</h4>
                        <p className="text-2xl font-bold text-red-600">{type.diffSummary.removedFields}</p>
                      </div>
                      <div className="border border-border rounded-lg p-4">
                        <h4 className="font-medium text-foreground mb-2">Modified Fields</h4>
                        <p className="text-2xl font-bold text-yellow-600">{type.diffSummary.modifiedFields}</p>
                      </div>
                    </div>
                    
                    <div className="border border-border rounded-lg p-4">
                      <h4 className="font-medium text-foreground mb-2">Last Modified</h4>
                      <p className="text-muted-foreground">
                        {new Date(type.diffSummary.lastModified).toLocaleString()}
                      </p>
                    </div>
                  </div>
                ) : (
                  <p className="text-muted-foreground">No diff information available for this type.</p>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
