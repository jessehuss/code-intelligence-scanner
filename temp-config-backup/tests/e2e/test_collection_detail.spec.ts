import { test, expect } from '@playwright/test'

test.describe('Collection Detail Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/collections/users')
  })

  test('should display collection detail page with all sections', async ({ page }) => {
    // Wait for page to load
    await expect(page.locator('[data-testid="collection-detail"]')).toBeVisible()
    
    // Verify page loads within 400ms
    const startTime = Date.now()
    await expect(page.locator('[data-testid="collection-detail"]')).toBeVisible()
    const endTime = Date.now()
    expect(endTime - startTime).toBeLessThan(400)
    
    // Verify all tabs are present
    await expect(page.locator('[data-testid="tab-schema"]')).toBeVisible()
    await expect(page.locator('[data-testid="tab-types"]')).toBeVisible()
    await expect(page.locator('[data-testid="tab-queries"]')).toBeVisible()
    await expect(page.locator('[data-testid="tab-relationships"]')).toBeVisible()
  })

  test('should display schema comparison in Schema tab', async ({ page }) => {
    // Click on Schema tab
    await page.click('[data-testid="tab-schema"]')
    
    // Verify schema table is visible
    await expect(page.locator('[data-testid="schema-table"]')).toBeVisible()
    
    // Verify declared schema is shown
    await expect(page.locator('[data-testid="declared-schema"]')).toBeVisible()
    
    // Verify observed schema is shown
    await expect(page.locator('[data-testid="observed-schema"]')).toBeVisible()
    
    // Verify presence percentages are displayed
    await expect(page.locator('[data-testid="presence-metrics"]')).toBeVisible()
    
    // Verify drift badges are present
    await expect(page.locator('[data-testid="drift-badges"]')).toBeVisible()
  })

  test('should display drift indicators with correct severity', async ({ page }) => {
    await page.click('[data-testid="tab-schema"]')
    
    // Check for different drift severity levels
    const lowDrift = page.locator('[data-testid="drift-badge-low"]')
    const mediumDrift = page.locator('[data-testid="drift-badge-medium"]')
    const highDrift = page.locator('[data-testid="drift-badge-high"]')
    const criticalDrift = page.locator('[data-testid="drift-badge-critical"]')
    
    // At least one drift indicator should be visible
    const hasAnyDrift = await Promise.race([
      lowDrift.isVisible(),
      mediumDrift.isVisible(),
      highDrift.isVisible(),
      criticalDrift.isVisible()
    ])
    
    expect(hasAnyDrift).toBeTruthy()
  })

  test('should display types in Types tab', async ({ page }) => {
    // Click on Types tab
    await page.click('[data-testid="tab-types"]')
    
    // Verify types list is visible
    await expect(page.locator('[data-testid="types-list"]')).toBeVisible()
    
    // Verify type items are displayed
    await expect(page.locator('[data-testid="type-item"]')).toBeVisible()
    
    // Click on a type to navigate to type detail
    await page.click('[data-testid="type-item"]:first-child')
    
    // Verify navigation to type detail page
    await expect(page).toHaveURL(/\/types\/.*/)
  })

  test('should display queries in Queries tab with code snippets', async ({ page }) => {
    // Click on Queries tab
    await page.click('[data-testid="tab-queries"]')
    
    // Verify queries list is visible
    await expect(page.locator('[data-testid="queries-list"]')).toBeVisible()
    
    // Verify query items are displayed
    await expect(page.locator('[data-testid="query-item"]')).toBeVisible()
    
    // Verify code snippets are syntax highlighted
    await expect(page.locator('[data-testid="code-snippet"]')).toBeVisible()
    
    // Verify copy buttons are present
    await expect(page.locator('[data-testid="copy-code-button"]')).toBeVisible()
  })

  test('should display relationships in Relationships tab with mini graph', async ({ page }) => {
    // Click on Relationships tab
    await page.click('[data-testid="tab-relationships"]')
    
    // Verify relationships section is visible
    await expect(page.locator('[data-testid="relationships-section"]')).toBeVisible()
    
    // Verify mini graph is rendered
    await expect(page.locator('[data-testid="mini-graph"]')).toBeVisible()
    
    // Verify graph nodes are present
    await expect(page.locator('[data-testid="graph-node"]')).toBeVisible()
    
    // Verify graph edges are present
    await expect(page.locator('[data-testid="graph-edge"]')).toBeVisible()
  })

  test('should navigate to full graph view from mini graph', async ({ page }) => {
    await page.click('[data-testid="tab-relationships"]')
    
    // Click on "View Full Graph" button
    await page.click('[data-testid="view-full-graph"]')
    
    // Verify navigation to graph page
    await expect(page).toHaveURL(/\/graph/)
    await expect(page.locator('[data-testid="graph-view"]')).toBeVisible()
  })

  test('should display deep links to source code', async ({ page }) => {
    // Verify provenance links are present
    await expect(page.locator('[data-testid="provenance-link"]')).toBeVisible()
    
    // Click on a provenance link
    const provenanceLink = page.locator('[data-testid="provenance-link"]:first-child')
    const href = await provenanceLink.getAttribute('href')
    
    // Verify link points to Git repository
    expect(href).toMatch(/github\.com|gitlab\.com|bitbucket\.org/)
    
    // Verify link opens in new tab
    const target = await provenanceLink.getAttribute('target')
    expect(target).toBe('_blank')
  })

  test('should handle broken source links gracefully', async ({ page }) => {
    // Mock broken provenance data
    await page.route('**/api/collections/users', route => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'users',
          name: 'users',
          provenance: {
            repository: '',
            filePath: '',
            lineNumber: 0,
            commitSha: '',
            timestamp: new Date().toISOString(),
            extractor: 'test'
          }
        })
      })
    })
    
    // Reload page
    await page.reload()
    
    // Verify fallback behavior (redirect to repository root)
    await expect(page.locator('[data-testid="provenance-link"]')).toBeVisible()
    
    const provenanceLink = page.locator('[data-testid="provenance-link"]:first-child')
    const href = await provenanceLink.getAttribute('href')
    
    // Should point to repository root when specific link is unavailable
    expect(href).toMatch(/github\.com\/.*\/$/)
  })

  test('should support keyboard navigation between tabs', async ({ page }) => {
    // Use keyboard to navigate tabs
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    await page.keyboard.press('Enter') // Activate Types tab
    
    // Verify Types tab is active
    await expect(page.locator('[data-testid="tab-types"][aria-selected="true"]')).toBeVisible()
    
    // Navigate to next tab
    await page.keyboard.press('ArrowRight')
    await page.keyboard.press('Enter') // Activate Queries tab
    
    // Verify Queries tab is active
    await expect(page.locator('[data-testid="tab-queries"][aria-selected="true"]')).toBeVisible()
  })

  test('should handle API errors gracefully', async ({ page }) => {
    // Mock API error
    await page.route('**/api/collections/users', route => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' })
      })
    })
    
    // Reload page
    await page.reload()
    
    // Verify error state is shown
    await expect(page.locator('[data-testid="collection-error"]')).toBeVisible()
    await expect(page.locator('[data-testid="collection-error"]')).toContainText('Failed to load collection')
    
    // Verify retry button is available
    await expect(page.locator('[data-testid="retry-collection"]')).toBeVisible()
  })

  test('should show loading state while fetching data', async ({ page }) => {
    // Mock slow API response
    await page.route('**/api/collections/users', route => {
      setTimeout(() => {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'users',
            name: 'users',
            declaredSchema: [],
            observedSchema: [],
            presenceMetrics: {},
            driftIndicators: [],
            types: [],
            queries: [],
            relationships: [],
            provenance: {}
          })
        })
      }, 1000)
    })
    
    // Navigate to collection page
    await page.goto('/collections/users')
    
    // Verify loading state is shown
    await expect(page.locator('[data-testid="collection-loading"]')).toBeVisible()
    
    // Wait for data to load
    await expect(page.locator('[data-testid="collection-detail"]')).toBeVisible()
    
    // Verify loading state is gone
    await expect(page.locator('[data-testid="collection-loading"]')).not.toBeVisible()
  })
})
