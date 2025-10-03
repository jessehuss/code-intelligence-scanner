// Base API response type
export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}

// Search related types
export interface SearchResult {
  id: string
  name: string
  kind: 'Collection' | 'Type' | 'Field' | 'Query' | 'Service'
  description?: string
  metadata?: Record<string, any>
}

export interface SearchFacets {
  repository: string[]
  service: string[]
  operation: string[]
  changedSince: string[]
}

export interface SearchPagination {
  page: number
  limit: number
  total: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
}

export interface SearchResponse {
  results: SearchResult[]
  facets: SearchFacets
  pagination: SearchPagination
  query: string
  executionTime: number
}

// Collection related types
export interface SchemaField {
  name: string
  type: string
  isRequired: boolean
  isArray: boolean
  nestedFields?: SchemaField[]
}

export interface PresenceMetrics {
  totalSamples: number
  presentSamples: number
  presencePercentage: number
  lastUpdated: string
}

export interface DriftIndicator {
  fieldName: string
  driftType: DriftType
  severity: DriftSeverity
  description: string
  suggestedAction: string
}

export interface TypeReference {
  id: string
  name: string
  namespace: string
  usageCount: number
}

export interface QueryReference {
  id: string
  name: string
  operation: QueryOperation
  usageCount: number
}

export interface Relationship {
  id: string
  sourceId: string
  targetId: string
  edgeKind: EdgeKind
  weight: number
  metadata?: Record<string, any>
}

export interface ProvenanceRecord {
  repository: string
  filePath: string
  lineNumber: number
  commitSha: string
  timestamp: string
  extractor: string
}

export interface Collection {
  id: string
  name: string
  declaredSchema: SchemaField[]
  observedSchema: SchemaField[]
  presenceMetrics: Record<string, PresenceMetrics>
  driftIndicators: DriftIndicator[]
  types: TypeReference[]
  queries: QueryReference[]
  relationships: Relationship[]
  provenance: ProvenanceRecord
}

// Type related types
export interface Field {
  id: string
  name: string
  type: string
  isRequired: boolean
  defaultValue?: any
  attributes: Attribute[]
  usagePatterns: UsagePattern[]
  provenance: ProvenanceRecord
}

export interface Attribute {
  name: string
  value: any
  parameters?: Record<string, any>
}

export interface UsagePattern {
  context: string
  frequency: number
  locations: Location[]
}

export interface Location {
  filePath: string
  lineNumber: number
  columnNumber?: number
}

export interface CollectionReference {
  id: string
  name: string
  usageCount: number
}

export interface Usage {
  context: string
  frequency: number
  locations: Location[]
}

export interface DiffSummary {
  fromSha: string
  toSha: string
  addedFields: Field[]
  removedFields: Field[]
  modifiedFields: Field[]
  changeCount: number
}

export interface Type {
  id: string
  name: string
  namespace: string
  fields: Field[]
  attributes: Attribute[]
  collections: CollectionReference[]
  usages: Usage[]
  diffSummary: DiffSummary
  relationships: Relationship[]
  provenance: ProvenanceRecord
}

// Graph related types
export interface GraphNode {
  id: string
  label: string
  type: 'Collection' | 'Type' | 'Field' | 'Query' | 'Service'
  metadata?: Record<string, any>
  x?: number
  y?: number
}

export interface GraphEdge {
  id: string
  source: string
  target: string
  edgeKind: EdgeKind
  weight: number
  metadata?: Record<string, any>
}

export interface GraphMetadata {
  totalNodes: number
  totalEdges: number
  nodeTypes: Record<string, number>
  edgeTypes: Record<string, number>
  lastUpdated: string
}

export interface GraphData {
  nodes: GraphNode[]
  edges: GraphEdge[]
  metadata: GraphMetadata
}

// Query helper types
export interface QueryHelperResponse {
  mongoShell: string
  csharpBuilder: string
  operation: QueryOperation
  fieldPath: string
  typeName: string
  examples: {
    mongoShell: string
    csharpBuilder: string
  }
}

// Enums
export enum QueryOperation {
  FIND = 'FIND',
  INSERT = 'INSERT',
  UPDATE = 'UPDATE',
  DELETE = 'DELETE',
  AGGREGATE = 'AGGREGATE'
}

export enum ServiceType {
  API = 'API',
  GRAPHQL = 'GRAPHQL',
  GRPC = 'GRPC',
  BACKGROUND = 'BACKGROUND',
  SCHEDULED = 'SCHEDULED'
}

export enum EdgeKind {
  USES = 'USES',
  CONTAINS = 'CONTAINS',
  REFERENCES = 'REFERENCES',
  IMPLEMENTS = 'IMPLEMENTS',
  EXTENDS = 'EXTENDS'
}

export enum DriftType {
  MISSING_FIELD = 'MISSING_FIELD',
  EXTRA_FIELD = 'EXTRA_FIELD',
  TYPE_MISMATCH = 'TYPE_MISMATCH',
  REQUIRED_MISMATCH = 'REQUIRED_MISMATCH'
}

export enum DriftSeverity {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL'
}

// UI specific types
export interface SearchFilters {
  repository?: string[]
  service?: string[]
  operation?: string[]
  changedSince?: string
}

export interface GraphFilters {
  edgeKind?: EdgeKind
  depth?: number
  focus?: string
}

export interface LoadingState {
  isLoading: boolean
  error?: string
  retry?: () => void
}

export interface ToastMessage {
  id: string
  type: 'success' | 'error' | 'warning' | 'info'
  title: string
  description?: string
  duration?: number
}

// Component props types
export interface SearchBarProps {
  onSearch: (query: string) => void
  placeholder?: string
  debounceMs?: number
  className?: string
}

export interface KindResultsProps {
  results: SearchResult[]
  kind: 'Collection' | 'Type' | 'Field' | 'Query' | 'Service'
  onItemClick: (item: SearchResult) => void
  className?: string
}

export interface FacetPanelProps {
  facets: SearchFacets
  selectedFacets: SearchFilters
  onFacetChange: (facets: SearchFilters) => void
  className?: string
}

export interface SchemaTableProps {
  declaredSchema: SchemaField[]
  observedSchema: SchemaField[]
  presenceMetrics: Record<string, PresenceMetrics>
  driftIndicators: DriftIndicator[]
  className?: string
}

export interface DriftBadgeProps {
  drift: DriftIndicator
  className?: string
}

export interface CodeSnippetProps {
  code: string
  language: string
  readOnly?: boolean
  onCopy?: () => void
  className?: string
}

export interface MiniGraphProps {
  nodes: GraphNode[]
  edges: GraphEdge[]
  onNodeClick?: (node: GraphNode) => void
  className?: string
}

export interface QueryHelperProps {
  typeName: string
  fieldPath: string
  onOperationChange: (operation: QueryOperation) => void
  className?: string
}

// Hook types
export interface UseSearchOptions {
  debounceMs?: number
  initialQuery?: string
}

export interface UseSearchReturn {
  query: string
  setQuery: (query: string) => void
  results: SearchResponse | null
  isLoading: boolean
  error: string | null
  search: (query: string) => void
}

export interface UseApiOptions {
  retryCount?: number
  retryDelay?: number
}

export interface UseApiReturn<T> {
  data: T | null
  isLoading: boolean
  error: string | null
  mutate: () => Promise<void>
  retry: () => void
}
