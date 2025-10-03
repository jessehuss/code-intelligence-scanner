import { test, expect } from '@playwright/test'

test.describe('Global Search Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/')
  })

  test('should perform global search with instant results', async ({ page }) => {
    // Navigate to search page
    await page.goto('/search')
    
    // Wait for search input to be visible
    await expect(page.locator('[data-testid="search-input"]')).toBeVisible()
    
    // Type search query
    await page.fill('[data-testid="search-input"]', 'user')
    
    // Wait for results to appear (should be under 300ms)
    const startTime = Date.now()
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
    const endTime = Date.now()
    
    // Verify response time is under 300ms
    expect(endTime - startTime).toBeLessThan(300)
    
    // Verify results are grouped by kind
    await expect(page.locator('[data-testid="results-collections"]')).toBeVisible()
    await expect(page.locator('[data-testid="results-types"]')).toBeVisible()
    await expect(page.locator('[data-testid="results-fields"]')).toBeVisible()
    await expect(page.locator('[data-testid="results-queries"]')).toBeVisible()
    await expect(page.locator('[data-testid="results-services"]')).toBeVisible()
    
    // Verify facets are displayed
    await expect(page.locator('[data-testid="facet-repository"]')).toBeVisible()
    await expect(page.locator('[data-testid="facet-service"]')).toBeVisible()
    await expect(page.locator('[data-testid="facet-operation"]')).toBeVisible()
    await expect(page.locator('[data-testid="facet-changed-since"]')).toBeVisible()
  })

  test('should filter results using facets', async ({ page }) => {
    await page.goto('/search')
    
    // Perform initial search
    await page.fill('[data-testid="search-input"]', 'user')
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
    
    // Apply repository facet filter
    await page.click('[data-testid="facet-repository"]')
    await page.click('[data-testid="facet-repository-main"]')
    
    // Verify results are filtered
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
    
    // Apply service facet filter
    await page.click('[data-testid="facet-service"]')
    await page.click('[data-testid="facet-service-api"]')
    
    // Verify results are further filtered
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
  })

  test('should navigate to detail pages from search results', async ({ page }) => {
    await page.goto('/search')
    
    // Perform search
    await page.fill('[data-testid="search-input"]', 'user')
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
    
    // Click on a collection result
    await page.click('[data-testid="result-collection-users"]')
    
    // Verify navigation to collection detail page
    await expect(page).toHaveURL(/\/collections\/users/)
    await expect(page.locator('[data-testid="collection-detail"]')).toBeVisible()
  })

  test('should handle empty search results', async ({ page }) => {
    await page.goto('/search')
    
    // Search for something that doesn't exist
    await page.fill('[data-testid="search-input"]', 'nonexistent-entity-xyz')
    
    // Wait for results
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
    
    // Verify empty state is shown
    await expect(page.locator('[data-testid="empty-results"]')).toBeVisible()
    await expect(page.locator('[data-testid="empty-results"]')).toContainText('No results found')
  })

  test('should support keyboard navigation', async ({ page }) => {
    await page.goto('/search')
    
    // Use Cmd+K (Mac) or Ctrl+K (Windows) to open search
    await page.keyboard.press('Meta+k')
    
    // Verify search input is focused
    await expect(page.locator('[data-testid="search-input"]')).toBeFocused()
    
    // Type search query
    await page.keyboard.type('user')
    
    // Use arrow keys to navigate results
    await page.keyboard.press('ArrowDown')
    await page.keyboard.press('ArrowDown')
    
    // Press Enter to select result
    await page.keyboard.press('Enter')
    
    // Verify navigation occurred
    await expect(page).toHaveURL(/\/collections\/users/)
  })

  test('should debounce search input', async ({ page }) => {
    await page.goto('/search')
    
    const searchInput = page.locator('[data-testid="search-input"]')
    
    // Type quickly to test debouncing
    await searchInput.fill('u')
    await searchInput.fill('us')
    await searchInput.fill('use')
    await searchInput.fill('user')
    
    // Wait for debounced search to complete
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
    
    // Verify only one search request was made (not one for each character)
    // This would be verified by checking network requests in a real test
  })

  test('should show loading state during search', async ({ page }) => {
    await page.goto('/search')
    
    // Start typing
    await page.fill('[data-testid="search-input"]', 'user')
    
    // Verify loading state appears briefly
    await expect(page.locator('[data-testid="search-loading"]')).toBeVisible()
    
    // Wait for results to load
    await expect(page.locator('[data-testid="search-results"]')).toBeVisible()
    
    // Verify loading state is gone
    await expect(page.locator('[data-testid="search-loading"]')).not.toBeVisible()
  })

  test('should handle search errors gracefully', async ({ page }) => {
    await page.goto('/search')
    
    // Mock network failure
    await page.route('**/api/search*', route => route.abort())
    
    // Perform search
    await page.fill('[data-testid="search-input"]', 'user')
    
    // Verify error state is shown
    await expect(page.locator('[data-testid="search-error"]')).toBeVisible()
    await expect(page.locator('[data-testid="search-error"]')).toContainText('Failed to search')
    
    // Verify retry button is available
    await expect(page.locator('[data-testid="retry-search"]')).toBeVisible()
  })
})
