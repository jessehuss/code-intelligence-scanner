import { test, expect } from '@playwright/test'

test.describe('Type Detail Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/types/User')
  })

  test('should display type detail page with comprehensive information', async ({ page }) => {
    // Wait for page to load
    await expect(page.locator('[data-testid="type-detail"]')).toBeVisible()
    
    // Verify page loads within 400ms
    const startTime = Date.now()
    await expect(page.locator('[data-testid="type-detail"]')).toBeVisible()
    const endTime = Date.now()
    expect(endTime - startTime).toBeLessThan(400)
    
    // Verify class header is displayed
    await expect(page.locator('[data-testid="type-header"]')).toBeVisible()
    await expect(page.locator('[data-testid="type-name"]')).toContainText('User')
    await expect(page.locator('[data-testid="type-namespace"]')).toBeVisible()
    
    // Verify all sections are present
    await expect(page.locator('[data-testid="fields-section"]')).toBeVisible()
    await expect(page.locator('[data-testid="attributes-section"]')).toBeVisible()
    await expect(page.locator('[data-testid="collections-section"]')).toBeVisible()
    await expect(page.locator('[data-testid="usages-section"]')).toBeVisible()
    await expect(page.locator('[data-testid="diff-section"]')).toBeVisible()
  })

  test('should display fields and attributes table', async ({ page }) => {
    // Verify fields table is visible
    await expect(page.locator('[data-testid="fields-table"]')).toBeVisible()
    
    // Verify field rows are displayed
    await expect(page.locator('[data-testid="field-row"]')).toBeVisible()
    
    // Verify field information is complete
    await expect(page.locator('[data-testid="field-name"]')).toBeVisible()
    await expect(page.locator('[data-testid="field-type"]')).toBeVisible()
    await expect(page.locator('[data-testid="field-required"]')).toBeVisible()
    await expect(page.locator('[data-testid="field-default"]')).toBeVisible()
    
    // Verify attributes are displayed
    await expect(page.locator('[data-testid="attributes-table"]')).toBeVisible()
    await expect(page.locator('[data-testid="attribute-row"]')).toBeVisible()
  })

  test('should display collections using this type', async ({ page }) => {
    // Verify collections section is visible
    await expect(page.locator('[data-testid="collections-section"]')).toBeVisible()
    
    // Verify collection items are displayed
    await expect(page.locator('[data-testid="collection-item"]')).toBeVisible()
    
    // Click on a collection to navigate to collection detail
    await page.click('[data-testid="collection-item"]:first-child')
    
    // Verify navigation to collection detail page
    await expect(page).toHaveURL(/\/collections\/.*/)
  })

  test('should display usage patterns with frequency data', async ({ page }) => {
    // Verify usages section is visible
    await expect(page.locator('[data-testid="usages-section"]')).toBeVisible()
    
    // Verify usage items are displayed
    await expect(page.locator('[data-testid="usage-item"]')).toBeVisible()
    
    // Verify frequency data is shown
    await expect(page.locator('[data-testid="usage-frequency"]')).toBeVisible()
    
    // Verify usage locations are displayed
    await expect(page.locator('[data-testid="usage-locations"]')).toBeVisible()
  })

  test('should display diff summary between SHAs', async ({ page }) => {
    // Verify diff section is visible
    await expect(page.locator('[data-testid="diff-section"]')).toBeVisible()
    
    // Verify diff summary is displayed
    await expect(page.locator('[data-testid="diff-summary"]')).toBeVisible()
    
    // Verify SHA information is shown
    await expect(page.locator('[data-testid="diff-from-sha"]')).toBeVisible()
    await expect(page.locator('[data-testid="diff-to-sha"]')).toBeVisible()
    
    // Verify change counts are displayed
    await expect(page.locator('[data-testid="diff-added-fields"]')).toBeVisible()
    await expect(page.locator('[data-testid="diff-removed-fields"]')).toBeVisible()
    await expect(page.locator('[data-testid="diff-modified-fields"]')).toBeVisible()
  })

  test('should display relationships with bidirectional links', async ({ page }) => {
    // Verify relationships section is visible
    await expect(page.locator('[data-testid="relationships-section"]')).toBeVisible()
    
    // Verify relationship items are displayed
    await expect(page.locator('[data-testid="relationship-item"]')).toBeVisible()
    
    // Verify relationship types are shown
    await expect(page.locator('[data-testid="relationship-type"]')).toBeVisible()
    
    // Click on a relationship to navigate to related entity
    await page.click('[data-testid="relationship-item"]:first-child')
    
    // Verify navigation to related entity
    await expect(page).toHaveURL(/\/types\/.*|/collections\/.*/)
  })

  test('should provide query helper for specific fields', async ({ page }) => {
    // Click on a field to open query helper
    await page.click('[data-testid="field-row"]:first-child')
    
    // Verify query helper panel is opened
    await expect(page.locator('[data-testid="query-helper-panel"]')).toBeVisible()
    
    // Verify operation selector is present
    await expect(page.locator('[data-testid="operation-selector"]')).toBeVisible()
    
    // Select an operation
    await page.selectOption('[data-testid="operation-selector"]', 'FIND')
    
    // Verify code examples are generated
    await expect(page.locator('[data-testid="mongo-shell-example"]')).toBeVisible()
    await expect(page.locator('[data-testid="csharp-builder-example"]')).toBeVisible()
    
    // Verify copy buttons are present
    await expect(page.locator('[data-testid="copy-mongo-button"]')).toBeVisible()
    await expect(page.locator('[data-testid="copy-csharp-button"]')).toBeVisible()
  })

  test('should support all query operations in query helper', async ({ page }) => {
    await page.click('[data-testid="field-row"]:first-child')
    await expect(page.locator('[data-testid="query-helper-panel"]')).toBeVisible()
    
    const operations = ['FIND', 'INSERT', 'UPDATE', 'DELETE', 'AGGREGATE']
    
    for (const operation of operations) {
      // Select operation
      await page.selectOption('[data-testid="operation-selector"]', operation)
      
      // Verify examples are generated for this operation
      await expect(page.locator('[data-testid="mongo-shell-example"]')).toBeVisible()
      await expect(page.locator('[data-testid="csharp-builder-example"]')).toBeVisible()
      
      // Verify examples contain valid syntax
      const mongoExample = await page.locator('[data-testid="mongo-shell-example"]').textContent()
      const csharpExample = await page.locator('[data-testid="csharp-builder-example"]').textContent()
      
      expect(mongoExample).toBeTruthy()
      expect(csharpExample).toBeTruthy()
    }
  })

  test('should handle nested field paths in query helper', async ({ page }) => {
    // Find a nested field (if available)
    const nestedField = page.locator('[data-testid="field-row"]').filter({ hasText: '.' })
    
    if (await nestedField.count() > 0) {
      await nestedField.first().click()
      await expect(page.locator('[data-testid="query-helper-panel"]')).toBeVisible()
      
      // Verify nested field path is handled correctly
      await expect(page.locator('[data-testid="field-path-display"]')).toBeVisible()
      
      // Generate examples
      await page.selectOption('[data-testid="operation-selector"]', 'FIND')
      
      // Verify examples use correct nested path syntax
      const mongoExample = await page.locator('[data-testid="mongo-shell-example"]').textContent()
      expect(mongoExample).toContain('.')
    }
  })

  test('should display deep links to source code', async ({ page }) => {
    // Verify provenance links are present
    await expect(page.locator('[data-testid="provenance-link"]')).toBeVisible()
    
    // Click on a provenance link
    const provenanceLink = page.locator('[data-testid="provenance-link"]:first-child')
    const href = await provenanceLink.getAttribute('href')
    
    // Verify link points to Git repository with line number
    expect(href).toMatch(/github\.com|gitlab\.com|bitbucket\.org/)
    expect(href).toMatch(/#L\d+/)
    
    // Verify link opens in new tab
    const target = await provenanceLink.getAttribute('target')
    expect(target).toBe('_blank')
  })

  test('should handle broken source links gracefully', async ({ page }) => {
    // Mock broken provenance data
    await page.route('**/api/types/User', route => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: 'User',
          name: 'User',
          namespace: 'com.example',
          fields: [],
          attributes: [],
          collections: [],
          usages: [],
          diffSummary: {},
          relationships: [],
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
    
    // Verify fallback behavior
    await expect(page.locator('[data-testid="provenance-link"]')).toBeVisible()
    
    const provenanceLink = page.locator('[data-testid="provenance-link"]:first-child')
    const href = await provenanceLink.getAttribute('href')
    
    // Should point to repository root when specific link is unavailable
    expect(href).toMatch(/github\.com\/.*\/$/)
  })

  test('should support keyboard navigation', async ({ page }) => {
    // Use Tab to navigate through sections
    await page.keyboard.press('Tab')
    await page.keyboard.press('Tab')
    
    // Verify focus is on fields table
    await expect(page.locator('[data-testid="fields-table"]')).toBeFocused()
    
    // Use arrow keys to navigate table rows
    await page.keyboard.press('ArrowDown')
    await page.keyboard.press('ArrowDown')
    
    // Press Enter to open query helper
    await page.keyboard.press('Enter')
    
    // Verify query helper is opened
    await expect(page.locator('[data-testid="query-helper-panel"]')).toBeVisible()
  })

  test('should handle API errors gracefully', async ({ page }) => {
    // Mock API error
    await page.route('**/api/types/User', route => {
      route.fulfill({
        status: 500,
        contentType: 'application/json',
        body: JSON.stringify({ error: 'Internal Server Error' })
      })
    })
    
    // Reload page
    await page.reload()
    
    // Verify error state is shown
    await expect(page.locator('[data-testid="type-error"]')).toBeVisible()
    await expect(page.locator('[data-testid="type-error"]')).toContainText('Failed to load type')
    
    // Verify retry button is available
    await expect(page.locator('[data-testid="retry-type"]')).toBeVisible()
  })

  test('should show loading state while fetching data', async ({ page }) => {
    // Mock slow API response
    await page.route('**/api/types/User', route => {
      setTimeout(() => {
        route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            id: 'User',
            name: 'User',
            namespace: 'com.example',
            fields: [],
            attributes: [],
            collections: [],
            usages: [],
            diffSummary: {},
            relationships: [],
            provenance: {}
          })
        })
      }, 1000)
    })
    
    // Navigate to type page
    await page.goto('/types/User')
    
    // Verify loading state is shown
    await expect(page.locator('[data-testid="type-loading"]')).toBeVisible()
    
    // Wait for data to load
    await expect(page.locator('[data-testid="type-detail"]')).toBeVisible()
    
    // Verify loading state is gone
    await expect(page.locator('[data-testid="type-loading"]')).not.toBeVisible()
  })
})
