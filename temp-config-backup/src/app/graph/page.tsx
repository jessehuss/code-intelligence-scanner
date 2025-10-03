'use client'

import { useState, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import { useGraphData } from '@/hooks/useApi'
import { buildGitUrl } from '@/lib/utils'
import { Network, Filter, Loader2, AlertCircle, ExternalLink } from 'lucide-react'

type FilterType = 'all' | 'collections' | 'types' | 'queries' | 'services'
type EdgeFilterType = 'all' | 'references' | 'inherits' | 'implements' | 'uses'

export default function GraphPage() {
  const router = useRouter()
  const graphRef = useRef<HTMLDivElement>(null)
  const [nodeFilter, setNodeFilter] = useState<FilterType>('all')
  const [edgeFilter, setEdgeFilter] = useState<EdgeFilterType>('all')
  const [selectedNode, setSelectedNode] = useState<any>(null)
  const [isFullscreen, setIsFullscreen] = useState(false)

  const { data: graphData, isLoading, error, retry } = useGraphData()

  const nodeFilters = [
    { id: 'all' as const, label: 'All', count: graphData?.nodes.length || 0 },
    { id: 'collections' as const, label: 'Collections', count: graphData?.nodes.filter(n => n.type === 'Collection').length || 0 },
    { id: 'types' as const, label: 'Types', count: graphData?.nodes.filter(n => n.type === 'Type').length || 0 },
    { id: 'queries' as const, label: 'Queries', count: graphData?.nodes.filter(n => n.type === 'Query').length || 0 },
    { id: 'services' as const, label: 'Services', count: graphData?.nodes.filter(n => n.type === 'Service').length || 0 }
  ]

  const edgeFilters = [
    { id: 'all' as const, label: 'All', count: graphData?.edges.length || 0 },
    { id: 'references' as const, label: 'References', count: graphData?.edges.filter(e => e.edgeKind === 'references').length || 0 },
    { id: 'inherits' as const, label: 'Inherits', count: graphData?.edges.filter(e => e.edgeKind === 'inherits').length || 0 },
    { id: 'implements' as const, label: 'Implements', count: graphData?.edges.filter(e => e.edgeKind === 'implements').length || 0 },
    { id: 'uses' as const, label: 'Uses', count: graphData?.edges.filter(e => e.edgeKind === 'uses').length || 0 }
  ]

  const filteredNodes = graphData?.nodes.filter(node => {
    if (nodeFilter === 'all') return true
    return node.type.toLowerCase() === nodeFilter
  }) || []

  const filteredEdges = graphData?.edges.filter(edge => {
    if (edgeFilter === 'all') return true
    return edge.edgeKind === edgeFilter
  }) || []

  const handleNodeClick = (node: any) => {
    setSelectedNode(node)
    if (node.type === 'Collection') {
      router.push(`/collections/${node.id}`)
    } else if (node.type === 'Type') {
      router.push(`/types/${node.id}`)
    }
  }

  const handleEdgeClick = (edge: any) => {
    console.log('Edge clicked:', edge)
  }

  const toggleFullscreen = () => {
    setIsFullscreen(!isFullscreen)
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center" data-testid="graph-loading">
        <div className="flex items-center gap-2 text-muted-foreground">
          <Loader2 className="h-5 w-5 animate-spin" />
          Loading graph...
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center" data-testid="graph-error">
        <div className="text-center">
          <AlertCircle className="h-12 w-12 text-destructive mx-auto mb-4" />
          <h3 className="text-lg font-medium text-foreground mb-2">Failed to load graph</h3>
          <p className="text-muted-foreground mb-4">{error}</p>
          <button
            onClick={retry}
            className="inline-flex items-center gap-2 px-4 py-2 bg-primary text-primary-foreground rounded-md hover:bg-primary/90 transition-colors"
            data-testid="retry-graph"
          >
            <AlertCircle className="h-4 w-4" />
            Try Again
          </button>
        </div>
      </div>
    )
  }

  if (!graphData) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <Network className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
          <h3 className="text-lg font-medium text-foreground mb-2">No graph data available</h3>
          <p className="text-muted-foreground mb-4">Unable to load graph visualization data.</p>
        </div>
      </div>
    )
  }

  return (
    <div className={`min-h-screen bg-background ${isFullscreen ? 'fixed inset-0 z-50' : ''}`}>
      <div className="container mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-foreground mb-2">Graph Visualization</h1>
              <p className="text-muted-foreground">
                Explore relationships between {graphData.nodes.length} entities
              </p>
            </div>
            
            <button
              onClick={toggleFullscreen}
              className="inline-flex items-center gap-2 px-3 py-2 text-sm border border-border rounded-md hover:bg-muted transition-colors"
              data-testid="toggle-fullscreen"
            >
              <Network className="h-4 w-4" />
              {isFullscreen ? 'Exit Fullscreen' : 'Fullscreen'}
            </button>
          </div>
        </div>

        {/* Filters */}
        <div className="mb-6 space-y-4">
          <div className="flex items-center gap-4">
            <Filter className="h-5 w-5 text-muted-foreground" />
            <span className="text-sm font-medium text-foreground">Filters:</span>
          </div>
          
          <div className="flex flex-wrap gap-4">
            {/* Node Filters */}
            <div className="space-y-2">
              <label className="text-sm font-medium text-muted-foreground">Nodes</label>
              <div className="flex gap-2">
                {nodeFilters.map((filter) => (
                  <button
                    key={filter.id}
                    onClick={() => setNodeFilter(filter.id)}
                    className={`
                      px-3 py-1 text-sm rounded-md transition-colors
                      ${nodeFilter === filter.id
                        ? 'bg-primary text-primary-foreground'
                        : 'bg-muted text-muted-foreground hover:bg-muted/80'
                      }
                    `}
                    data-testid={`node-filter-${filter.id}`}
                  >
                    {filter.label} ({filter.count})
                  </button>
                ))}
              </div>
            </div>

            {/* Edge Filters */}
            <div className="space-y-2">
              <label className="text-sm font-medium text-muted-foreground">Edges</label>
              <div className="flex gap-2">
                {edgeFilters.map((filter) => (
                  <button
                    key={filter.id}
                    onClick={() => setEdgeFilter(filter.id)}
                    className={`
                      px-3 py-1 text-sm rounded-md transition-colors
                      ${edgeFilter === filter.id
                        ? 'bg-primary text-primary-foreground'
                        : 'bg-muted text-muted-foreground hover:bg-muted/80'
                      }
                    `}
                    data-testid={`edge-filter-${filter.id}`}
                  >
                    {filter.label} ({filter.count})
                  </button>
                ))}
              </div>
            </div>
          </div>
        </div>

        {/* Graph Container */}
        <div 
          ref={graphRef}
          className="border border-border rounded-lg bg-background"
          style={{ height: isFullscreen ? 'calc(100vh - 200px)' : '600px' }}
          data-testid="graph-container"
        >
          {/* Placeholder for Cytoscape.js graph */}
          <div className="w-full h-full flex items-center justify-center text-muted-foreground">
            <div className="text-center">
              <Network className="h-16 w-16 mx-auto mb-4" />
              <p>Graph visualization will be rendered here</p>
              <p className="text-sm mt-2">
                Showing {filteredNodes.length} nodes and {filteredEdges.length} edges
              </p>
            </div>
          </div>
        </div>

        {/* Selected Node Info */}
        {selectedNode && (
          <div className="mt-6 border border-border rounded-lg p-4" data-testid="selected-node-info">
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-lg font-medium text-foreground">Selected Node</h3>
              <button
                onClick={() => setSelectedNode(null)}
                className="text-sm text-muted-foreground hover:text-foreground"
              >
                Clear
              </button>
            </div>
            
            <div className="space-y-2">
              <div>
                <span className="text-sm font-medium text-muted-foreground">Type:</span>{' '}
                <span className="text-foreground">{selectedNode.type}</span>
              </div>
              <div>
                <span className="text-sm font-medium text-muted-foreground">ID:</span>{' '}
                <span className="text-foreground">{selectedNode.id}</span>
              </div>
              {selectedNode.label && (
                <div>
                  <span className="text-sm font-medium text-muted-foreground">Label:</span>{' '}
                  <span className="text-foreground">{selectedNode.label}</span>
                </div>
              )}
              {selectedNode.provenance && (
                <div>
                  <a
                    href={buildGitUrl(selectedNode.provenance)}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1 text-sm text-primary hover:text-primary/80"
                  >
                    <ExternalLink className="h-3 w-3" />
                    View Source
                  </a>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Graph Statistics */}
        <div className="mt-6 grid gap-4 md:grid-cols-4">
          <div className="border border-border rounded-lg p-4">
            <h4 className="font-medium text-foreground mb-2">Total Nodes</h4>
            <p className="text-2xl font-bold text-primary">{graphData.nodes.length}</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <h4 className="font-medium text-foreground mb-2">Total Edges</h4>
            <p className="text-2xl font-bold text-primary">{graphData.edges.length}</p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <h4 className="font-medium text-foreground mb-2">Collections</h4>
            <p className="text-2xl font-bold text-primary">
              {graphData.nodes.filter(n => n.type === 'Collection').length}
            </p>
          </div>
          <div className="border border-border rounded-lg p-4">
            <h4 className="font-medium text-foreground mb-2">Types</h4>
            <p className="text-2xl font-bold text-primary">
              {graphData.nodes.filter(n => n.type === 'Type').length}
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
