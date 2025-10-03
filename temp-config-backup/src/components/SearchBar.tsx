'use client'

import { useState, useEffect, useRef } from 'react'
import { Search, X } from 'lucide-react'
import { cn, debounce } from '@/lib/utils'

interface SearchBarProps {
  onSearch: (query: string) => void
  placeholder?: string
  debounceMs?: number
  className?: string
  initialQuery?: string
}

export function SearchBar({
  onSearch,
  placeholder = 'Search collections, types, fields, queries, and services...',
  debounceMs = 300,
  className,
  initialQuery = ''
}: SearchBarProps) {
  const [query, setQuery] = useState(initialQuery)
  const [isFocused, setIsFocused] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  // Debounced search function
  const debouncedSearch = useRef(
    debounce((searchQuery: string) => {
      onSearch(searchQuery)
    }, debounceMs)
  ).current

  // Handle input change
  const handleInputChange = (value: string) => {
    setQuery(value)
    debouncedSearch(value)
  }

  // Handle clear
  const handleClear = () => {
    setQuery('')
    onSearch('')
    inputRef.current?.focus()
  }

  // Handle keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      // Cmd+K or Ctrl+K to focus search
      if ((event.metaKey || event.ctrlKey) && event.key === 'k') {
        event.preventDefault()
        inputRef.current?.focus()
      }
      
      // Escape to clear search
      if (event.key === 'Escape' && isFocused) {
        handleClear()
      }
    }

    document.addEventListener('keydown', handleKeyDown)
    return () => document.removeEventListener('keydown', handleKeyDown)
  }, [isFocused])

  // Update query when initialQuery changes
  useEffect(() => {
    if (initialQuery !== query) {
      setQuery(initialQuery)
    }
  }, [initialQuery])

  return (
    <div className={cn('relative w-full max-w-2xl', className)}>
      <div className="relative">
        <Search 
          className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" 
        />
        <input
          ref={inputRef}
          type="text"
          value={query}
          onChange={(e) => handleInputChange(e.target.value)}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
          placeholder={placeholder}
          className={cn(
            'w-full rounded-lg border border-input bg-background px-10 py-3 text-sm',
            'placeholder:text-muted-foreground',
            'focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
            'disabled:cursor-not-allowed disabled:opacity-50',
            'transition-colors duration-200'
          )}
          data-testid="search-input"
        />
        {query && (
          <button
            onClick={handleClear}
            className="absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground hover:text-foreground"
            aria-label="Clear search"
            data-testid="clear-search"
          >
            <X className="h-4 w-4" />
          </button>
        )}
      </div>
      
      {/* Keyboard shortcut hint */}
      {!isFocused && (
        <div className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-muted-foreground">
          <kbd className="pointer-events-none hidden h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium opacity-100 sm:flex">
            <span className="text-xs">⌘</span>K
          </kbd>
        </div>
      )}
    </div>
  )
}
