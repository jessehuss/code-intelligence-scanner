import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'

const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'Catalog Explorer',
  description: 'Search-first web app for discovering Collections, Types, Fields, Queries, and Services in your codebase',
  keywords: ['catalog', 'explorer', 'search', 'codebase', 'types', 'collections'],
  authors: [{ name: 'Upshop Team' }],
  viewport: 'width=device-width, initial-scale=1',
  robots: 'index, follow',
  openGraph: {
    title: 'Catalog Explorer',
    description: 'Search-first web app for discovering Collections, Types, Fields, Queries, and Services in your codebase',
    type: 'website',
    locale: 'en_US',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Catalog Explorer',
    description: 'Search-first web app for discovering Collections, Types, Fields, Queries, and Services in your codebase',
  },
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={inter.className}>
        <div id="root">
          <ErrorBoundary>
            <Navigation />
            <main className="min-h-screen">
              {children}
            </main>
          </ErrorBoundary>
        </div>
      </body>
    </html>
  )
}

// Error Boundary Component
function ErrorBoundary({ children }: { children: React.ReactNode }) {
  return (
    <div className="error-boundary">
      {children}
    </div>
  )
}

// Navigation Component
function Navigation() {
  return (
    <nav className="border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <div className="flex items-center gap-4">
            <a
              href="/"
              className="flex items-center gap-2 text-lg font-semibold text-foreground hover:text-primary transition-colors"
            >
              <svg
                className="h-6 w-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
              </svg>
              Catalog Explorer
            </a>
          </div>

          {/* Navigation Links */}
          <div className="flex items-center gap-6">
            <a
              href="/search"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Search
            </a>
            <a
              href="/graph"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Graph
            </a>
            <a
              href="/collections"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Collections
            </a>
            <a
              href="/types"
              className="text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              Types
            </a>
          </div>

          {/* Search Bar (Global) */}
          <div className="flex-1 max-w-md mx-8">
            <div className="relative">
              <input
                type="text"
                placeholder="Search collections, types, fields..."
                className="w-full px-4 py-2 pl-10 text-sm border border-border rounded-md bg-background text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    const query = (e.target as HTMLInputElement).value
                    if (query.trim()) {
                      window.location.href = `/search?q=${encodeURIComponent(query.trim())}`
                    }
                  }
                }}
              />
              <svg
                className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                />
              </svg>
            </div>
          </div>

          {/* Theme Toggle (Placeholder) */}
          <div className="flex items-center gap-2">
            <button
              className="p-2 text-muted-foreground hover:text-foreground transition-colors"
              aria-label="Toggle theme"
            >
              <svg
                className="h-4 w-4"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"
                />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </nav>
  )
}
