# Quickstart: F003 – Catalog Explorer UI

**Date**: 2024-12-19  
**Feature**: Catalog Explorer UI  
**Phase**: 1 - Quickstart Guide

## Overview

The Catalog Explorer UI is a search-first web application that allows backend developers and SREs to discover and explore Collections, Types, Fields, Queries, and Services in their codebase. This quickstart guide demonstrates the core user flows and validates the feature implementation.

## Prerequisites

- Node.js 18+ and npm/yarn
- Access to the F002 Catalog API
- Modern web browser (Chrome, Firefox, Safari, Edge)

## Installation & Setup

### 1. Clone and Install Dependencies

```bash
# Navigate to the catalog explorer directory
cd apps/catalog-explorer

# Install dependencies
npm install

# Set environment variables
cp .env.example .env.local
# Edit .env.local with your API configuration
```

### 2. Environment Configuration

```bash
# .env.local
NEXT_PUBLIC_API_BASE_URL=http://localhost:8080/api
NEXT_PUBLIC_REPO_BASE_URL=https://github.com/your-org/your-repo
```

### 3. Start Development Server

```bash
npm run dev
```

The application will be available at `http://localhost:3000`.

## User Flow Validation

### Flow 1: Global Search Discovery

**Objective**: Validate instant search functionality with federated results

**Steps**:
1. Navigate to `http://localhost:3000`
2. Click in the search bar or press `Cmd+K` (Mac) / `Ctrl+K` (Windows)
3. Type "user" in the search box
4. Observe search results appearing within 300ms
5. Verify results are grouped by kind (Collections, Types, Fields, Queries, Services)
6. Check that facets are displayed (Repository, Service, Operation, Changed Since)
7. Click on a Collection result to navigate to detail page

**Expected Results**:
- Search results load within 300ms P50
- Results are properly grouped by entity type
- Facets are functional and filter results
- Navigation to detail pages works correctly

### Flow 2: Collection Detail Exploration

**Objective**: Validate collection detail page with schema comparison and drift indicators

**Steps**:
1. From search results, click on a Collection (e.g., "users")
2. Navigate to Collection Detail page: `/collections/users`
3. Review the Schema tab showing declared vs observed schema
4. Check presence percentages for each field
5. Look for drift badges indicating schema differences
6. Examine the Types section showing related types
7. Review the Queries section with code snippets
8. Check the Relationships section with mini graph
9. Click on a deep link to view source code

**Expected Results**:
- Page loads within 400ms P50 after cache warm
- Schema comparison is clearly displayed
- Presence percentages are accurate (0-100%)
- Drift indicators are properly categorized by severity
- Code snippets are syntax-highlighted
- Deep links navigate to correct source locations
- Mini graph shows relevant relationships

### Flow 3: Type Detail Analysis

**Objective**: Validate type detail page with comprehensive type information

**Steps**:
1. From search results or collection detail, click on a Type
2. Navigate to Type Detail page: `/types/User`
3. Review the class header with namespace and metadata
4. Examine the fields/attributes table
5. Check the Collections section showing usage
6. Review the Usages section with frequency data
7. Look at the Diff Summary between SHAs
8. Click on relationship links to explore connections
9. Use the Query Helper for a specific field

**Expected Results**:
- Type information is comprehensive and accurate
- Field attributes are properly displayed
- Collection usage is correctly tracked
- Usage patterns show meaningful data
- Diff summaries highlight relevant changes
- Relationships are bidirectional and accurate

### Flow 4: Graph Visualization

**Objective**: Validate interactive graph view with filtering capabilities

**Steps**:
1. Navigate to Graph View: `/graph`
2. Observe the force graph visualization
3. Use the edge type filter (USES, CONTAINS, REFERENCES, etc.)
4. Adjust the depth slider (1-5 levels)
5. Click on a node to focus the graph
6. Pan and zoom the graph
7. Click on nodes to navigate to detail pages
8. Test different filter combinations

**Expected Results**:
- Graph renders smoothly with up to 10,000 entities
- Filtering works correctly and updates the graph
- Depth limiting prevents overwhelming visualizations
- Node interactions are responsive
- Navigation from graph to detail pages works
- Graph performance remains acceptable with large datasets

### Flow 5: Query Helper Usage

**Objective**: Validate query generation for Mongo shell and C# Builders<T>

**Steps**:
1. Navigate to a Type Detail page
2. Select a specific field (e.g., "Email" field in "User" type)
3. Open the Query Helper panel
4. Select an operation (FIND, INSERT, UPDATE, DELETE, AGGREGATE)
5. Review the generated Mongo shell example
6. Review the generated C# Builders<T> example
7. Click "Copy as Mongo Shell" button
8. Click "Copy as C# Builder" button
9. Paste the copied code to verify it's valid

**Expected Results**:
- Query examples are generated correctly for all operations
- Mongo shell examples are syntactically valid
- C# Builders<T> examples are syntactically valid
- Copy functionality works correctly
- Examples are executable and follow best practices
- Field paths are properly handled (including nested paths)

## Performance Validation

### Search Performance
- **Target**: Search results within 300ms P50
- **Test**: Perform 10 searches with different query lengths
- **Validation**: Use browser dev tools to measure response times
- **Acceptance**: 80% of searches complete within 300ms

### Page Load Performance
- **Target**: Detail pages within 400ms P50 after cache warm
- **Test**: Navigate to 10 different collection and type pages
- **Validation**: Measure page load times after initial cache
- **Acceptance**: 80% of pages load within 400ms

### Graph Performance
- **Target**: Smooth interaction with up to 10,000 entities
- **Test**: Load graph with maximum entities and interact
- **Validation**: Monitor frame rates during pan/zoom operations
- **Acceptance**: Maintain 30+ FPS during interactions

## Error Handling Validation

### Network Errors
1. Disconnect network during search
2. Verify error toast appears
3. Check that retry functionality works
4. Confirm graceful degradation

### API Errors
1. Simulate 404 responses
2. Verify appropriate error messages
3. Check that fallback content is shown
4. Confirm error boundaries work correctly

### Broken Source Links
1. Navigate to entities with invalid provenance
2. Verify redirect to repository root
3. Check that error handling is graceful
4. Confirm user feedback is provided

## Accessibility Validation

### Keyboard Navigation
1. Navigate entire application using only keyboard
2. Verify Tab order is logical
3. Check that focus indicators are visible
4. Confirm all interactive elements are accessible

### Screen Reader Support
1. Use screen reader to navigate the application
2. Verify semantic HTML is used correctly
3. Check that ARIA labels are present
4. Confirm complex interactions are properly described

## Browser Compatibility

Test the application in:
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

Verify that all functionality works consistently across browsers.

## Success Criteria

The quickstart is successful when:

1. ✅ All user flows complete without errors
2. ✅ Performance targets are met consistently
3. ✅ Error handling works gracefully
4. ✅ Accessibility requirements are satisfied
5. ✅ Copy-as functionality provides valid code
6. ✅ Deep links navigate to correct locations
7. ✅ Graph visualization handles large datasets
8. ✅ Search provides relevant, fast results

## Troubleshooting

### Common Issues

**Search not working**:
- Check API connection in browser dev tools
- Verify environment variables are set correctly
- Ensure F002 Catalog API is running

**Graph not rendering**:
- Check browser console for JavaScript errors
- Verify Cytoscape.js is loaded correctly
- Ensure data format matches expected schema

**Copy-as not working**:
- Check browser clipboard permissions
- Verify Monaco Editor is initialized
- Ensure code generation is working correctly

**Performance issues**:
- Check network tab for slow API responses
- Verify caching is working correctly
- Monitor memory usage in large graphs

### Getting Help

- Check browser console for error messages
- Review API logs for backend issues
- Consult the data model documentation
- Refer to the contract tests for expected behavior
