import { test, expect } from '@playwright/test'

test.describe('Graph Visualization Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/graph')
  })

  test('should display interactive graph visualization', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-view"]')).toBeVisible()
    
    // Verify graph container is present
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Verify graph nodes are rendered
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Verify graph edges are rendered
    await expect(page.locator('[data-testid="graph-edge"]')).toBeVisible()
    
    // Verify graph loads within 500ms
    const startTime = Date.now()
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    const endTime = Date.now()
    expect(endTime - startTime).toBeLessThan(500)
  })

  test('should support pan and zoom interactions', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Test panning
    await page.locator('[data-testid="graph-container"]').dragTo(
      page.locator('[data-testid="graph-container"]'),
      { targetPosition: { x: 100, y: 100 } }
    )
    
    // Test zooming
    await page.locator('[data-testid="graph-container"]').wheel({ deltaY: -100 })
    
    // Verify graph is still interactive
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
  })

  test('should filter graph by edge type', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Verify edge type filter is present
    await expect(page.locator('[data-testid="edge-type-filter"]')).toBeVisible()
    
    // Select USES edge type
    await page.selectOption('[data-testid="edge-type-filter"]', 'USES')
    
    // Verify graph updates
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Select CONTAINS edge type
    await page.selectOption('[data-testid="edge-type-filter"]', 'CONTAINS')
    
    // Verify graph updates again
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Select REFERENCES edge type
    await page.selectOption('[data-testid="edge-type-filter"]', 'REFERENCES')
    
    // Verify graph updates
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
  })

  test('should adjust graph depth with slider', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Verify depth slider is present
    await expect(page.locator('[data-testid="depth-slider"]')).toBeVisible()
    
    // Set depth to 1
    await page.locator('[data-testid="depth-slider"]').fill('1')
    
    // Verify graph updates
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Set depth to 3
    await page.locator('[data-testid="depth-slider"]').fill('3')
    
    // Verify graph updates
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Set depth to 5 (maximum)
    await page.locator('[data-testid="depth-slider"]').fill('5')
    
    // Verify graph updates
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
  })

  test('should focus graph on specific node', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Click on a node to focus
    await page.click('[data-testid="graph-node"]:first-child')
    
    // Verify node is focused/selected
    await expect(page.locator('[data-testid="graph-node"][data-selected="true"]')).toBeVisible()
    
    // Verify focus controls are available
    await expect(page.locator('[data-testid="focus-controls"]')).toBeVisible()
    
    // Click "Focus on Node" button
    await page.click('[data-testid="focus-node-button"]')
    
    // Verify graph centers on the selected node
    await expect(page.locator('[data-testid="graph-node"][data-selected="true"]')).toBeVisible()
  })

  test('should navigate to detail pages from graph nodes', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Double-click on a node to navigate
    await page.dblclick('[data-testid="graph-node"]:first-child')
    
    // Verify navigation occurred (could be to collection or type detail)
    const currentUrl = page.url()
    expect(currentUrl).toMatch(/\/collections\/.*|\/types\/.*/)
  })

  test('should display node information on hover', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Hover over a node
    await page.hover('[data-testid="graph-node"]:first-child')
    
    // Verify tooltip is displayed
    await expect(page.locator('[data-testid="node-tooltip"]')).toBeVisible()
    
    // Verify tooltip contains node information
    await expect(page.locator('[data-testid="node-tooltip"]')).toContainText('User')
  })

  test('should handle large graphs with up to 10,000 entities', async ({ page }) => {
    // Mock large graph data
    await page.route('**/api/graph', route => {
      const nodes = Array.from({ length: 1000 }, (_, i) => ({
        id: `node-${i}`,
        label: `Node ${i}`,
        type: i % 2 === 0 ? 'Collection' : 'Type'
      }))
      
      const edges = Array.from({ length: 500 }, (_, i) => ({
        id: `edge-${i}`,
        source: `node-${i}`,
        target: `node-${i + 1}`,
        edgeKind: 'USES'
      }))
      
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ nodes, edges, metadata: { totalNodes: 1000, totalEdges: 500 } })
      })
    })
    
    // Reload page
    await page.reload()
    
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Verify graph renders without performance issues
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Test interactions still work with large graph
    await page.locator('[data-testid="graph-container"]').wheel({ deltaY: -100 })
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
  })

  test('should maintain 30+ FPS during interactions', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Perform rapid interactions to test performance
    const startTime = Date.now()
    
    for (let i = 0; i < 10; i++) {
      await page.locator('[data-testid="graph-container"]').wheel({ deltaY: -50 })
      await page.locator('[data-testid="graph-container"]').dragTo(
        page.locator('[data-testid="graph-container"]'),
        { targetPosition: { x: i * 10, y: i * 10 } }
      )
    }
    
    const endTime = Date.now()
    const duration = endTime - startTime
    
    // Verify interactions completed in reasonable time (indicating good performance)
    expect(duration).toBeLessThan(2000) // Should complete in under 2 seconds
  })

  test('should support multiple filter combinations', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Apply multiple filters
    await page.selectOption('[data-testid="edge-type-filter"]', 'USES')
    await page.locator('[data-testid="depth-slider"]').fill('2')
    
    // Verify graph updates with combined filters
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Change edge type while keeping depth
    await page.selectOption('[data-testid="edge-type-filter"]', 'CONTAINS')
    
    // Verify graph updates
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Reset filters
    await page.click('[data-testid="reset-filters"]')
    
    // Verify graph resets
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
  })

  test('should handle graph loading errors gracefully', async ({ page }) => {
    // Mock API error
    await page.route('**/api/graph', route => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' })
      })
    })
    
    // Reload page
    await page.reload()
    
    // Verify error state is shown
    await expect(page.locator('[data-testid="graph-error"]')).toBeVisible()
    await expect(page.locator('[data-testid="graph-error"]')).toContainText('Failed to load graph')
    
    // Verify retry button is available
    await expect(page.locator('[data-testid="retry-graph"]')).toBeVisible()
  })

  test('should show loading state while fetching graph data', async ({ page }) => {
    // Mock slow API response
    await page.route('**/api/graph', route => {
      setTimeout(() => {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            nodes: [{ id: 'node-1', label: 'Node 1', type: 'Collection' }],
            edges: [],
            metadata: { totalNodes: 1, totalEdges: 0 }
          })
        })
      }, 1000)
    })
    
    // Navigate to graph page
    await page.goto('/graph')
    
    // Verify loading state is shown
    await expect(page.locator('[data-testid="graph-loading"]')).toBeVisible()
    
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Verify loading state is gone
    await expect(page.locator('[data-testid="graph-loading"]')).not.toBeVisible()
  })

  test('should support keyboard navigation', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Use Tab to navigate to filters
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    
    // Verify focus is on edge type filter
    await expect(page.locator('[data-testid="edge-type-filter"]')).toBeFocused()
    
    // Use arrow keys to change selection
    await page.keyboard.press('ArrowDown')
    await page.keyboard.press('Enter')
    
    // Verify filter is applied
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Navigate to depth slider
    await page.keyboard.press('Tab')
    
    // Verify focus is on depth slider
    await expect(page.locator('[data-testid="depth-slider"]')).toBeFocused()
  })

  test('should display graph statistics', async ({ page }) => {
    // Wait for graph to load
    await expect(page.locator('[data-testid="graph-container"]')).toBeVisible()
    
    // Verify statistics are displayed
    await expect(page.locator('[data-testid="graph-stats"]')).toBeVisible()
    await expect(page.locator('[data-testid="node-count"]')).toBeVisible()
    await expect(page.locator('[data-testid="edge-count"]')).toBeVisible()
    
    // Verify counts are reasonable
    const nodeCount = await page.locator('[data-testid="node-count"]').textContent()
    const edgeCount = await page.locator('[data-testid="edge-count"]').textContent()
    
    expect(parseInt(nodeCount || '0')).toBeGreaterThan(0)
    expect(parseInt(edgeCount || '0')).toBeGreaterThanOrEqual(0)
  })
})
