# Catalog Explorer

A search-first web application for backend developers and SREs to discover Collections, Types, Fields, Queries, and Services in their codebase.

## Features

- **Global Search**: Instant search across Collections, Types, Fields, Queries, and Services with federated results
- **Graph Visualization**: Interactive force graph showing relationships between entities with filtering capabilities
- **Collection Details**: Comprehensive view of collection schemas with drift indicators and usage statistics
- **Type Information**: Detailed type definitions with field information, usage tracking, and relationship mapping
- **Query Helper**: Generates Mongo shell and C# Builders<T> examples
- **Deep Linking**: Direct links to Git source lines via provenance data

## Tech Stack

- **Frontend**: Next.js 14+ (App Router), TypeScript, Tailwind CSS, shadcn/ui
- **Data Fetching**: SWR for caching and real-time updates
- **Code Editor**: Monaco Editor for code snippets
- **Graph Visualization**: Cytoscape.js for interactive graphs
- **Testing**: Vitest, React Testing Library, Playwright
- **Performance**: Lighthouse budgeting, <300ms search, <400ms page loads

## Getting Started

### Prerequisites

- Node.js 18+ (recommended: 20+)
- npm or yarn
- Access to the F002 Catalog API

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd apps/catalog-explorer
```

2. Install dependencies:
```bash
npm install
```

3. Set up environment variables:
```bash
cp env.example .env.local
# Edit .env.local with your API configuration
```

4. Start the development server:
```bash
npm run dev
```

5. Open [http://localhost:3000](http://localhost:3000) in your browser.

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_CATALOG_API_BASE_URL` | Base URL for the F002 Catalog API | `http://localhost:5000/api` |

## Development

### Available Scripts

- `npm run dev` - Start development server with Turbopack
- `npm run build` - Build for production
- `npm run start` - Start production server
- `npm run lint` - Run ESLint
- `npm run test` - Run unit tests with Vitest
- `npm run test:ui` - Run tests with UI
- `npm run test:e2e` - Run end-to-end tests with Playwright
- `npm run format` - Format code with Prettier
- `npm run format:check` - Check code formatting

### Project Structure

```
src/
├── app/                    # Next.js App Router pages
│   ├── search/            # Global search page
│   ├── collections/[name]/ # Collection detail pages
│   ├── types/[fqcn]/      # Type detail pages
│   ├── graph/             # Graph visualization page
│   └── layout.tsx         # Root layout
├── components/            # Reusable UI components
│   ├── ui/               # shadcn/ui components
│   ├── SearchBar.tsx     # Global search component
│   ├── KindResults.tsx   # Search results by kind
│   ├── FacetPanel.tsx    # Search facets
│   ├── SchemaTable.tsx   # Schema comparison table
│   ├── DriftBadge.tsx    # Drift indicator badge
│   ├── CodeSnippet.tsx   # Monaco editor wrapper
│   ├── MiniGraph.tsx     # Cytoscape mini graph
│   └── QueryHelper.tsx   # Query generation helper
├── lib/                  # Utilities and configurations
│   ├── api.ts           # F002 API client
│   ├── utils.ts         # Common utilities
│   └── types.ts         # TypeScript type definitions
└── hooks/               # Custom React hooks
    ├── useSearch.ts     # Search functionality
    ├── useApi.ts        # Data fetching
    └── useKeyboard.ts   # Keyboard navigation
```

## Testing

### Unit Tests

Run unit tests with Vitest:

```bash
npm run test
```

### End-to-End Tests

Run E2E tests with Playwright:

```bash
npm run test:e2e
```

### Contract Tests

Contract tests ensure the frontend correctly interacts with the F002 Catalog API:

```bash
# These tests should initially fail as the API client is not yet implemented
npm run test:contract
```

## Performance

The application is optimized for:

- **Search Response**: <300ms P50
- **Page Load**: <400ms P50 after cache warm-up
- **Concurrent Users**: 10-50 users
- **Data Scale**: Up to 10,000 entities

## Deployment

### Docker

Build and run with Docker:

```bash
docker build -t catalog-explorer .
docker run -p 3000:3000 catalog-explorer
```

### Environment Configuration

Ensure the following environment variables are set in production:

- `NEXT_PUBLIC_CATALOG_API_BASE_URL` - Production API URL
- `NODE_ENV=production`

## API Integration

This application consumes the F002 Catalog API. Ensure the API is running and accessible at the configured base URL.

### API Endpoints

- `GET /api/search` - Global search across all entities
- `GET /api/collections/:name` - Collection details
- `GET /api/types/:fqcn` - Type details
- `GET /api/graph` - Graph data
- `GET /api/query-helper` - Query examples

## Contributing

1. Follow the existing code style and patterns
2. Write tests for new features
3. Ensure all tests pass before submitting
4. Update documentation as needed

## License

[Add your license information here]
