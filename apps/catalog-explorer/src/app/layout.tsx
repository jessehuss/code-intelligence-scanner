import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import Link from "next/link";
import { Search, Network, Database, FileText, Home } from "lucide-react";

const inter = Inter({
  subsets: ["latin"],
  variable: "--font-inter",
});

export const metadata: Metadata = {
  title: "Catalog Explorer",
  description: "Search-first web app for backend developers and SREs to discover Collections, Types, Fields, Queries, and Services in your codebase.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={`${inter.variable} font-sans antialiased`}>
        {/* Navigation */}
        <nav className="border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
          <div className="container mx-auto px-4">
            <div className="flex h-16 items-center justify-between">
              <div className="flex items-center space-x-8">
                <Link href="/" className="flex items-center space-x-2">
                  <Database className="h-6 w-6 text-primary" />
                  <span className="text-xl font-bold text-foreground">Catalog Explorer</span>
                </Link>
                <div className="hidden md:flex items-center space-x-6">
                  <Link
                    href="/search"
                    className="flex items-center space-x-1 text-sm text-muted-foreground hover:text-foreground transition-colors"
                  >
                    <Search className="h-4 w-4" />
                    <span>Search</span>
                  </Link>
                  <Link
                    href="/graph"
                    className="flex items-center space-x-1 text-sm text-muted-foreground hover:text-foreground transition-colors"
                  >
                    <Network className="h-4 w-4" />
                    <span>Graph</span>
                  </Link>
                  <Link
                    href="/search?kind=collection"
                    className="flex items-center space-x-1 text-sm text-muted-foreground hover:text-foreground transition-colors"
                  >
                    <Database className="h-4 w-4" />
                    <span>Collections</span>
                  </Link>
                  <Link
                    href="/search?kind=type"
                    className="flex items-center space-x-1 text-sm text-muted-foreground hover:text-foreground transition-colors"
                  >
                    <FileText className="h-4 w-4" />
                    <span>Types</span>
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </nav>

        {/* Main Content */}
        <main className="min-h-screen">
          {children}
        </main>

        {/* Footer */}
        <footer className="border-t border-border bg-muted/50">
          <div className="container mx-auto px-4 py-8">
            <div className="text-center text-sm text-muted-foreground">
              <p>Catalog Explorer - Discover your codebase structure and relationships</p>
            </div>
          </div>
        </footer>
      </body>
    </html>
  );
}