import { ApiResponse, SearchResponse, Collection, Type, GraphData, QueryHelperResponse } from './types'

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:8080/api'

class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public response?: Response
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const errorText = await response.text()
    throw new ApiError(
      `API request failed: ${response.status} ${response.statusText}`,
      response.status,
      response
    )
  }

  try {
    return await response.json()
  } catch (error) {
    throw new ApiError('Failed to parse API response', response.status, response)
  }
}

async function fetchWithRetry<T>(
  url: string,
  options: RequestInit = {},
  maxRetries: number = 3
): Promise<T> {
  let lastError: Error

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      const response = await fetch(url, {
        ...options,
        headers: {
          'Content-Type': 'application/json',
          ...options.headers,
        },
      })

      return await handleResponse<T>(response)
    } catch (error) {
      lastError = error as Error
      
      // Don't retry on client errors (4xx)
      if (error instanceof ApiError && error.status >= 400 && error.status < 500) {
        throw error
      }

      // Wait before retrying (exponential backoff)
      if (attempt < maxRetries) {
        const delay = Math.pow(2, attempt) * 1000
        await new Promise(resolve => setTimeout(resolve, delay))
      }
    }
  }

  throw lastError!
}

export class CatalogApiClient {
  private baseUrl: string

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl
  }

  async search(
    query: string,
    options: {
      facets?: Record<string, string[]>
      page?: number
      limit?: number
    } = {}
  ): Promise<SearchResponse> {
    const params = new URLSearchParams({
      q: query,
      ...(options.page && { page: options.page.toString() }),
      ...(options.limit && { limit: options.limit.toString() }),
    })

    if (options.facets) {
      Object.entries(options.facets).forEach(([key, values]) => {
        values.forEach(value => params.append('facets', `${key}:${value}`))
      })
    }

    return fetchWithRetry<SearchResponse>(`${this.baseUrl}/search?${params}`)
  }

  async getCollection(name: string): Promise<Collection> {
    return fetchWithRetry<Collection>(`${this.baseUrl}/collections/${encodeURIComponent(name)}`)
  }

  async getCollectionSchema(name: string): Promise<{
    declaredSchema: any[]
    observedSchema: any[]
    driftIndicators: any[]
  }> {
    return fetchWithRetry(`${this.baseUrl}/collections/${encodeURIComponent(name)}/schema`)
  }

  async getCollectionTypes(name: string): Promise<{ types: any[] }> {
    return fetchWithRetry(`${this.baseUrl}/collections/${encodeURIComponent(name)}/types`)
  }

  async getCollectionQueries(name: string): Promise<{ queries: any[] }> {
    return fetchWithRetry(`${this.baseUrl}/collections/${encodeURIComponent(name)}/queries`)
  }

  async getCollectionRelationships(name: string): Promise<{ relationships: any[] }> {
    return fetchWithRetry(`${this.baseUrl}/collections/${encodeURIComponent(name)}/relationships`)
  }

  async getType(fqcn: string): Promise<Type> {
    return fetchWithRetry<Type>(`${this.baseUrl}/types/${encodeURIComponent(fqcn)}`)
  }

  async getGraph(options: {
    edgeKind?: string
    depth?: number
    focus?: string
  } = {}): Promise<GraphData> {
    const params = new URLSearchParams()
    
    if (options.edgeKind) params.append('edgeKind', options.edgeKind)
    if (options.depth) params.append('depth', options.depth.toString())
    if (options.focus) params.append('focus', options.focus)

    const queryString = params.toString()
    const url = queryString ? `${this.baseUrl}/graph?${queryString}` : `${this.baseUrl}/graph`

    return fetchWithRetry<GraphData>(url)
  }

  async generateQuery(options: {
    type: string
    field: string
    operation: 'FIND' | 'INSERT' | 'UPDATE' | 'DELETE' | 'AGGREGATE'
  }): Promise<QueryHelperResponse> {
    const params = new URLSearchParams({
      type: options.type,
      field: options.field,
      operation: options.operation,
    })

    return fetchWithRetry<QueryHelperResponse>(`${this.baseUrl}/query-helper?${params}`)
  }
}

// Export singleton instance
export const apiClient = new CatalogApiClient()

// Export error class for error handling
export { ApiError }
