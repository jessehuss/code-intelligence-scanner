'use client'

import { useEffect, useRef, useState } from 'react'
import { GraphNode, GraphEdge } from '@/lib/types'
import { cn } from '@/lib/utils'

interface MiniGraphProps {
  nodes: GraphNode[]
  edges: GraphEdge[]
  onNodeClick?: (node: GraphNode) => void
  className?: string
  height?: string | number
}

export function MiniGraph({
  nodes,
  edges,
  onNodeClick,
  className,
  height = 300
}: MiniGraphProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cytoscape: any = null

    const initGraph = async () => {
      try {
        setIsLoading(true)
        setError(null)

        // Dynamically import Cytoscape
        const cytoscapeModule = await import('cytoscape')
        const cytoscapeCore = cytoscapeModule.default

        if (!containerRef.current) return

        // Initialize Cytoscape
        cytoscape = cytoscapeCore({
          container: containerRef.current,
          elements: [
            ...nodes.map(node => ({
              data: {
                id: node.id,
                label: node.label,
                type: node.type,
                ...node.metadata
              }
            })),
            ...edges.map(edge => ({
              data: {
                id: edge.id,
                source: edge.source,
                target: edge.target,
                edgeKind: edge.edgeKind,
                weight: edge.weight,
                ...edge.metadata
              }
            }))
          ],
          style: [
            {
              selector: 'node',
              style: {
                'background-color': '#3b82f6',
                'label': 'data(label)',
                'text-valign': 'center',
                'text-halign': 'center',
                'color': '#ffffff',
                'font-size': '12px',
                'font-weight': 'bold',
                'text-outline-width': 2,
                'text-outline-color': '#1e40af',
                'width': '60px',
                'height': '60px',
                'border-width': 2,
                'border-color': '#1e40af'
              }
            },
            {
              selector: 'node[type="Collection"]',
              style: {
                'background-color': '#3b82f6',
                'border-color': '#1e40af'
              }
            },
            {
              selector: 'node[type="Type"]',
              style: {
                'background-color': '#10b981',
                'border-color': '#059669'
              }
            },
            {
              selector: 'node[type="Field"]',
              style: {
                'background-color': '#8b5cf6',
                'border-color': '#7c3aed'
              }
            },
            {
              selector: 'node[type="Query"]',
              style: {
                'background-color': '#f59e0b',
                'border-color': '#d97706'
              }
            },
            {
              selector: 'node[type="Service"]',
              style: {
                'background-color': '#ef4444',
                'border-color': '#dc2626'
              }
            },
            {
              selector: 'edge',
              style: {
                'width': 2,
                'line-color': '#6b7280',
                'target-arrow-color': '#6b7280',
                'target-arrow-shape': 'triangle',
                'curve-style': 'bezier',
                'opacity': 0.7
              }
            },
            {
              selector: 'edge[edgeKind="USES"]',
              style: {
                'line-color': '#3b82f6',
                'target-arrow-color': '#3b82f6'
              }
            },
            {
              selector: 'edge[edgeKind="CONTAINS"]',
              style: {
                'line-color': '#10b981',
                'target-arrow-color': '#10b981'
              }
            },
            {
              selector: 'edge[edgeKind="REFERENCES"]',
              style: {
                'line-color': '#8b5cf6',
                'target-arrow-color': '#8b5cf6'
              }
            },
            {
              selector: 'edge[edgeKind="IMPLEMENTS"]',
              style: {
                'line-color': '#f59e0b',
                'target-arrow-color': '#f59e0b'
              }
            },
            {
              selector: 'edge[edgeKind="EXTENDS"]',
              style: {
                'line-color': '#ef4444',
                'target-arrow-color': '#ef4444'
              }
            }
          ],
          layout: {
            name: 'cose',
            idealEdgeLength: 100,
            nodeOverlap: 20,
            refresh: 20,
            fit: true,
            padding: 30,
            randomize: false,
            componentSpacing: 100,
            nodeRepulsion: 400000,
            edgeElasticity: 100,
            nestingFactor: 5,
            gravity: 80,
            numIter: 1000,
            initialTemp: 200,
            coolingFactor: 0.95,
            minTemp: 1.0
          }
        })

        // Add event listeners
        cytoscape.on('tap', 'node', (event: any) => {
          const nodeData = event.target.data()
          const node = nodes.find(n => n.id === nodeData.id)
          if (node && onNodeClick) {
            onNodeClick(node)
          }
        })

        // Fit the graph to the container
        cytoscape.fit()

        setIsLoading(false)
      } catch (err) {
        console.error('Failed to initialize graph:', err)
        setError('Failed to load graph visualization')
        setIsLoading(false)
      }
    }

    initGraph()

    return () => {
      if (cytoscape) {
        cytoscape.destroy()
      }
    }
  }, [nodes, edges, onNodeClick])

  if (error) {
    return (
      <div className={cn('flex items-center justify-center rounded-lg border border-border bg-muted/50', className)}>
        <div className="text-center p-4">
          <div className="text-sm text-muted-foreground mb-2">Graph visualization unavailable</div>
          <div className="text-xs text-muted-foreground">{error}</div>
        </div>
      </div>
    )
  }

  return (
    <div className={cn('relative rounded-lg border border-border overflow-hidden', className)}>
      <div className="bg-muted/50 px-4 py-2 border-b border-border">
        <h3 className="text-sm font-medium text-foreground">Relationships</h3>
        <p className="text-xs text-muted-foreground mt-1">
          {nodes.length} nodes, {edges.length} relationships
        </p>
      </div>
      
      <div 
        ref={containerRef}
        className="w-full bg-background"
        style={{ height }}
        data-testid="mini-graph"
      />
      
      {isLoading && (
        <div className="absolute inset-0 bg-background/50 flex items-center justify-center">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <div className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
            Loading graph...
          </div>
        </div>
      )}
      
      {/* Legend */}
      <div className="absolute bottom-4 left-4 bg-background/90 backdrop-blur-sm rounded-lg border border-border p-3">
        <div className="text-xs font-medium text-foreground mb-2">Node Types</div>
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-blue-500" />
            <span className="text-xs text-muted-foreground">Collection</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-green-500" />
            <span className="text-xs text-muted-foreground">Type</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-purple-500" />
            <span className="text-xs text-muted-foreground">Field</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-orange-500" />
            <span className="text-xs text-muted-foreground">Query</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full bg-red-500" />
            <span className="text-xs text-muted-foreground">Service</span>
          </div>
        </div>
      </div>
    </div>
  )
}
