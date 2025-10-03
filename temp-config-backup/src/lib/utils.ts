import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// Debounce utility for search input
export function debounce<T extends (...args: any[]) => any>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout | null = null
  
  return (...args: Parameters<T>) => {
    if (timeout) {
      clearTimeout(timeout)
    }
    
    timeout = setTimeout(() => {
      func(...args)
    }, wait)
  }
}

// Deep link utilities
export function buildGitUrl(provenance: {
  repository: string
  filePath: string
  lineNumber: number
  commitSha?: string
}): string {
  const { repository, filePath, lineNumber, commitSha } = provenance
  
  if (!repository || !filePath) {
    // Fallback to repository root
    return repository || 'https://github.com'
  }
  
  // Detect Git hosting provider
  if (repository.includes('github.com')) {
    const baseUrl = commitSha 
      ? `${repository}/blob/${commitSha}/${filePath}`
      : `${repository}/blob/main/${filePath}`
    return lineNumber > 0 ? `${baseUrl}#L${lineNumber}` : baseUrl
  }
  
  if (repository.includes('gitlab.com')) {
    const baseUrl = commitSha
      ? `${repository}/-/blob/${commitSha}/${filePath}`
      : `${repository}/-/blob/main/${filePath}`
    return lineNumber > 0 ? `${baseUrl}#L${lineNumber}` : baseUrl
  }
  
  if (repository.includes('bitbucket.org')) {
    const baseUrl = commitSha
      ? `${repository}/src/${commitSha}/${filePath}`
      : `${repository}/src/main/${filePath}`
    return lineNumber > 0 ? `${baseUrl}#lines-${lineNumber}` : baseUrl
  }
  
  // Fallback to repository root
  return repository
}

// Copy to clipboard utility
export async function copyToClipboard(text: string): Promise<boolean> {
  try {
    if (navigator.clipboard && window.isSecureContext) {
      await navigator.clipboard.writeText(text)
      return true
    } else {
      // Fallback for older browsers
      const textArea = document.createElement('textarea')
      textArea.value = text
      textArea.style.position = 'fixed'
      textArea.style.left = '-999999px'
      textArea.style.top = '-999999px'
      document.body.appendChild(textArea)
      textArea.focus()
      textArea.select()
      
      const success = document.execCommand('copy')
      document.body.removeChild(textArea)
      return success
    }
  } catch (error) {
    console.error('Failed to copy to clipboard:', error)
    return false
  }
}

// Format utilities
export function formatNumber(num: number): string {
  if (num >= 1000000) {
    return (num / 1000000).toFixed(1) + 'M'
  }
  if (num >= 1000) {
    return (num / 1000).toFixed(1) + 'K'
  }
  return num.toString()
}

export function formatPercentage(value: number): string {
  return `${Math.round(value)}%`
}

export function formatDate(date: string | Date): string {
  const d = new Date(date)
  return d.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

export function formatRelativeTime(date: string | Date): string {
  const d = new Date(date)
  const now = new Date()
  const diffInSeconds = Math.floor((now.getTime() - d.getTime()) / 1000)
  
  if (diffInSeconds < 60) {
    return 'just now'
  }
  
  const diffInMinutes = Math.floor(diffInSeconds / 60)
  if (diffInMinutes < 60) {
    return `${diffInMinutes}m ago`
  }
  
  const diffInHours = Math.floor(diffInMinutes / 60)
  if (diffInHours < 24) {
    return `${diffInHours}h ago`
  }
  
  const diffInDays = Math.floor(diffInHours / 24)
  if (diffInDays < 7) {
    return `${diffInDays}d ago`
  }
  
  return formatDate(d)
}

// Validation utilities
export function isValidQuery(query: string): boolean {
  return query.trim().length > 0 && query.trim().length <= 100
}

export function isValidCollectionName(name: string): boolean {
  return /^[a-zA-Z][a-zA-Z0-9_-]*$/.test(name)
}

export function isValidTypeName(name: string): boolean {
  return /^[a-zA-Z][a-zA-Z0-9_.]*$/.test(name)
}

// Array utilities
export function groupBy<T, K extends string | number>(
  array: T[],
  key: (item: T) => K
): Record<K, T[]> {
  return array.reduce((groups, item) => {
    const groupKey = key(item)
    if (!groups[groupKey]) {
      groups[groupKey] = []
    }
    groups[groupKey].push(item)
    return groups
  }, {} as Record<K, T[]>)
}

export function sortBy<T>(
  array: T[],
  key: (item: T) => string | number,
  direction: 'asc' | 'desc' = 'asc'
): T[] {
  return [...array].sort((a, b) => {
    const aVal = key(a)
    const bVal = key(b)
    
    if (aVal < bVal) return direction === 'asc' ? -1 : 1
    if (aVal > bVal) return direction === 'asc' ? 1 : -1
    return 0
  })
}

// Performance utilities
export function measurePerformance<T>(
  name: string,
  fn: () => T
): T {
  const start = performance.now()
  const result = fn()
  const end = performance.now()
  
  console.log(`${name} took ${end - start} milliseconds`)
  return result
}

export async function measureAsyncPerformance<T>(
  name: string,
  fn: () => Promise<T>
): Promise<T> {
  const start = performance.now()
  const result = await fn()
  const end = performance.now()
  
  console.log(`${name} took ${end - start} milliseconds`)
  return result
}

// Error handling utilities
export function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message
  }
  if (typeof error === 'string') {
    return error
  }
  return 'An unknown error occurred'
}

export function isApiError(error: unknown): error is { status: number; message: string } {
  return (
    typeof error === 'object' &&
    error !== null &&
    'status' in error &&
    'message' in error
  )
}

// Local storage utilities
export function getFromLocalStorage<T>(key: string, defaultValue: T): T {
  if (typeof window === 'undefined') {
    return defaultValue
  }
  
  try {
    const item = localStorage.getItem(key)
    return item ? JSON.parse(item) : defaultValue
  } catch (error) {
    console.error(`Failed to get ${key} from localStorage:`, error)
    return defaultValue
  }
}

export function setToLocalStorage<T>(key: string, value: T): void {
  if (typeof window === 'undefined') {
    return
  }
  
  try {
    localStorage.setItem(key, JSON.stringify(value))
  } catch (error) {
    console.error(`Failed to set ${key} to localStorage:`, error)
  }
}

// URL utilities
export function buildSearchUrl(query: string, filters?: Record<string, string[]>): string {
  const params = new URLSearchParams()
  params.set('q', query)
  
  if (filters) {
    Object.entries(filters).forEach(([key, values]) => {
      values.forEach(value => params.append('facets', `${key}:${value}`))
    })
  }
  
  return `/search?${params.toString()}`
}

export function parseSearchUrl(searchParams: URLSearchParams): {
  query: string
  filters: Record<string, string[]>
} {
  const query = searchParams.get('q') || ''
  const filters: Record<string, string[]> = {}
  
  searchParams.getAll('facets').forEach(facet => {
    const [key, value] = facet.split(':')
    if (key && value) {
      if (!filters[key]) {
        filters[key] = []
      }
      filters[key].push(value)
    }
  })
  
  return { query, filters }
}
