'use client'

import { useState, useEffect, useCallback } from 'react'
import { ApiError } from '@/lib/api'

interface UseApiOptions {
  retryCount?: number
  retryDelay?: number
  enabled?: boolean
}

interface UseApiReturn<T> {
  data: T | null
  isLoading: boolean
  error: string | null
  mutate: () => Promise<void>
  retry: () => void
}

export function useApi<T>(
  apiCall: () => Promise<T>,
  options: UseApiOptions = {}
): UseApiReturn<T> {
  const { retryCount = 3, retryDelay = 1000, enabled = true } = options
  
  const [data, setData] = useState<T | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const executeApiCall = useCallback(async (attempt: number = 0): Promise<void> => {
    if (!enabled) return

    setIsLoading(true)
    setError(null)

    try {
      const result = await apiCall()
      setData(result)
    } catch (err) {
      const errorMessage = err instanceof ApiError 
        ? err.message 
        : err instanceof Error 
        ? err.message 
        : 'An unknown error occurred'

      // Don't retry on client errors (4xx)
      if (err instanceof ApiError && err.status >= 400 && err.status < 500) {
        setError(errorMessage)
        setIsLoading(false)
        return
      }

      // Retry on server errors or network issues
      if (attempt < retryCount) {
        setTimeout(() => {
          executeApiCall(attempt + 1)
        }, retryDelay * Math.pow(2, attempt)) // Exponential backoff
      } else {
        setError(errorMessage)
      }
    } finally {
      setIsLoading(false)
    }
  }, [apiCall, enabled, retryCount, retryDelay])

  const mutate = useCallback(() => executeApiCall(), [executeApiCall])
  
  const retry = useCallback(() => {
    setError(null)
    executeApiCall()
  }, [executeApiCall])

  // Execute on mount if enabled
  useEffect(() => {
    if (enabled) {
      executeApiCall()
    }
  }, [enabled, executeApiCall])

  return {
    data,
    isLoading,
    error,
    mutate,
    retry
  }
}

// Specialized hooks for common API patterns
export function useCollection(name: string) {
  return useApi(() => apiClient.getCollection(name), {
    enabled: !!name
  })
}

export function useType(fqcn: string) {
  return useApi(() => apiClient.getType(fqcn), {
    enabled: !!fqcn
  })
}

export function useGraph(options: { edgeKind?: string; depth?: number; focus?: string } = {}) {
  return useApi(() => apiClient.getGraph(options))
}
