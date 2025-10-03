'use client'

import { SchemaField, PresenceMetrics, DriftIndicator } from '@/lib/types'
import { cn, formatPercentage } from '@/lib/utils'
import { AlertTriangle, CheckCircle, XCircle, Info } from 'lucide-react'

interface SchemaTableProps {
  declaredSchema: SchemaField[]
  observedSchema: SchemaField[]
  presenceMetrics: Record<string, PresenceMetrics>
  driftIndicators: DriftIndicator[]
  className?: string
}

const getDriftIcon = (severity: string) => {
  switch (severity) {
    case 'CRITICAL':
      return <XCircle className="h-4 w-4 text-red-500" />
    case 'HIGH':
      return <AlertTriangle className="h-4 w-4 text-orange-500" />
    case 'MEDIUM':
      return <AlertTriangle className="h-4 w-4 text-yellow-500" />
    case 'LOW':
      return <Info className="h-4 w-4 text-blue-500" />
    default:
      return <CheckCircle className="h-4 w-4 text-green-500" />
  }
}

const getDriftColor = (severity: string) => {
  switch (severity) {
    case 'CRITICAL':
      return 'border-red-200 bg-red-50 text-red-800'
    case 'HIGH':
      return 'border-orange-200 bg-orange-50 text-orange-800'
    case 'MEDIUM':
      return 'border-yellow-200 bg-yellow-50 text-yellow-800'
    case 'LOW':
      return 'border-blue-200 bg-blue-50 text-blue-800'
    default:
      return 'border-green-200 bg-green-50 text-green-800'
  }
}

export function SchemaTable({
  declaredSchema,
  observedSchema,
  presenceMetrics,
  driftIndicators,
  className
}: SchemaTableProps) {
  // Create a map of all fields from both schemas
  const allFields = new Map<string, {
    name: string
    declared?: SchemaField
    observed?: SchemaField
    presence?: PresenceMetrics
    drift?: DriftIndicator
  }>()

  // Add declared fields
  declaredSchema.forEach(field => {
    allFields.set(field.name, {
      name: field.name,
      declared: field,
      presence: presenceMetrics[field.name]
    })
  })

  // Add observed fields
  observedSchema.forEach(field => {
    const existing = allFields.get(field.name)
    if (existing) {
      existing.observed = field
    } else {
      allFields.set(field.name, {
        name: field.name,
        observed: field,
        presence: presenceMetrics[field.name]
      })
    }
  })

  // Add drift indicators
  driftIndicators.forEach(drift => {
    const field = allFields.get(drift.fieldName)
    if (field) {
      field.drift = drift
    }
  })

  const fields = Array.from(allFields.values())

  return (
    <div className={cn('space-y-4', className)}>
      {/* Schema Comparison Table */}
      <div className="rounded-lg border border-border overflow-hidden">
        <div className="bg-muted/50 px-4 py-3 border-b border-border">
          <h3 className="text-sm font-medium text-foreground">Schema Comparison</h3>
          <p className="text-xs text-muted-foreground mt-1">
            Comparing declared schema with observed schema from data sampling
          </p>
        </div>
        
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-muted/30">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wide">
                  Field Name
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wide">
                  Declared Type
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wide">
                  Observed Type
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wide">
                  Required
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wide">
                  Presence
                </th>
                <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wide">
                  Status
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {fields.map((field) => (
                <tr key={field.name} className="hover:bg-muted/30">
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <code className="text-sm font-mono text-foreground">
                        {field.name}
                      </code>
                      {field.drift && (
                        <div className="flex items-center gap-1">
                          {getDriftIcon(field.drift.severity)}
                        </div>
                      )}
                    </div>
                  </td>
                  
                  <td className="px-4 py-3">
                    {field.declared ? (
                      <div className="space-y-1">
                        <code className="text-sm font-mono text-foreground">
                          {field.declared.type}
                        </code>
                        {field.declared.isArray && (
                          <span className="inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-800">
                            Array
                          </span>
                        )}
                      </div>
                    ) : (
                      <span className="text-sm text-muted-foreground italic">
                        Not declared
                      </span>
                    )}
                  </td>
                  
                  <td className="px-4 py-3">
                    {field.observed ? (
                      <div className="space-y-1">
                        <code className="text-sm font-mono text-foreground">
                          {field.observed.type}
                        </code>
                        {field.observed.isArray && (
                          <span className="inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs text-blue-800">
                            Array
                          </span>
                        )}
                      </div>
                    ) : (
                      <span className="text-sm text-muted-foreground italic">
                        Not observed
                      </span>
                    )}
                  </td>
                  
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      {field.declared?.isRequired && (
                        <span className="inline-flex items-center rounded-full bg-red-100 px-2 py-0.5 text-xs text-red-800">
                          Required
                        </span>
                      )}
                      {field.observed?.isRequired && !field.declared?.isRequired && (
                        <span className="inline-flex items-center rounded-full bg-orange-100 px-2 py-0.5 text-xs text-orange-800">
                          Observed Required
                        </span>
                      )}
                    </div>
                  </td>
                  
                  <td className="px-4 py-3">
                    {field.presence ? (
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <span className="text-sm text-foreground">
                            {formatPercentage(field.presence.presencePercentage)}
                          </span>
                          <div className="w-16 bg-muted rounded-full h-2">
                            <div
                              className="bg-primary h-2 rounded-full transition-all duration-300"
                              style={{ width: `${field.presence.presencePercentage}%` }}
                            />
                          </div>
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {field.presence.presentSamples} of {field.presence.totalSamples} samples
                        </div>
                      </div>
                    ) : (
                      <span className="text-sm text-muted-foreground">No data</span>
                    )}
                  </td>
                  
                  <td className="px-4 py-3">
                    {field.drift ? (
                      <div className={cn(
                        'inline-flex items-center gap-1 rounded-full border px-2 py-1 text-xs font-medium',
                        getDriftColor(field.drift.severity)
                      )}>
                        {getDriftIcon(field.drift.severity)}
                        {field.drift.driftType.replace('_', ' ')}
                      </div>
                    ) : (
                      <div className="inline-flex items-center gap-1 rounded-full border border-green-200 bg-green-50 px-2 py-1 text-xs font-medium text-green-800">
                        <CheckCircle className="h-3 w-3" />
                        Consistent
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Drift Indicators Summary */}
      {driftIndicators.length > 0 && (
        <div className="rounded-lg border border-border overflow-hidden">
          <div className="bg-muted/50 px-4 py-3 border-b border-border">
            <h3 className="text-sm font-medium text-foreground">Drift Indicators</h3>
            <p className="text-xs text-muted-foreground mt-1">
              Schema inconsistencies that require attention
            </p>
          </div>
          
          <div className="p-4 space-y-3">
            {driftIndicators.map((drift, index) => (
              <div
                key={index}
                className={cn(
                  'rounded-lg border p-3',
                  getDriftColor(drift.severity)
                )}
                data-testid={`drift-badge-${drift.severity.toLowerCase()}`}
              >
                <div className="flex items-start gap-2">
                  {getDriftIcon(drift.severity)}
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{drift.fieldName}</span>
                      <span className="text-xs opacity-75">
                        {drift.driftType.replace('_', ' ')}
                      </span>
                    </div>
                    <p className="text-sm mt-1 opacity-90">
                      {drift.description}
                    </p>
                    {drift.suggestedAction && (
                      <p className="text-xs mt-2 opacity-75">
                        <strong>Suggestion:</strong> {drift.suggestedAction}
                      </p>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
