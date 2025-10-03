'use client'

import { DriftIndicator, DriftSeverity } from '@/lib/types'
import { cn } from '@/lib/utils'
import { AlertTriangle, XCircle, Info, CheckCircle } from 'lucide-react'

interface DriftBadgeProps {
  drift: DriftIndicator
  className?: string
  showDescription?: boolean
  size?: 'sm' | 'md' | 'lg'
}

const getDriftConfig = (severity: DriftSeverity) => {
  switch (severity) {
    case 'CRITICAL':
      return {
        icon: XCircle,
        color: 'border-red-200 bg-red-50 text-red-800',
        iconColor: 'text-red-500',
        label: 'Critical'
      }
    case 'HIGH':
      return {
        icon: AlertTriangle,
        color: 'border-orange-200 bg-orange-50 text-orange-800',
        iconColor: 'text-orange-500',
        label: 'High'
      }
    case 'MEDIUM':
      return {
        icon: AlertTriangle,
        color: 'border-yellow-200 bg-yellow-50 text-yellow-800',
        iconColor: 'text-yellow-500',
        label: 'Medium'
      }
    case 'LOW':
      return {
        icon: Info,
        color: 'border-blue-200 bg-blue-50 text-blue-800',
        iconColor: 'text-blue-500',
        label: 'Low'
      }
    default:
      return {
        icon: CheckCircle,
        color: 'border-green-200 bg-green-50 text-green-800',
        iconColor: 'text-green-500',
        label: 'None'
      }
  }
}

const getSizeClasses = (size: 'sm' | 'md' | 'lg') => {
  switch (size) {
    case 'sm':
      return {
        container: 'px-2 py-1 text-xs',
        icon: 'h-3 w-3',
        text: 'text-xs'
      }
    case 'md':
      return {
        container: 'px-3 py-1.5 text-sm',
        icon: 'h-4 w-4',
        text: 'text-sm'
      }
    case 'lg':
      return {
        container: 'px-4 py-2 text-base',
        icon: 'h-5 w-5',
        text: 'text-base'
      }
  }
}

export function DriftBadge({
  drift,
  className,
  showDescription = false,
  size = 'md'
}: DriftBadgeProps) {
  const config = getDriftConfig(drift.severity)
  const sizeClasses = getSizeClasses(size)
  const Icon = config.icon

  if (showDescription) {
    return (
      <div
        className={cn(
          'rounded-lg border p-3 transition-colors',
          config.color,
          className
        )}
        data-testid={`drift-badge-${drift.severity.toLowerCase()}`}
      >
        <div className="flex items-start gap-2">
          <Icon className={cn('mt-0.5', sizeClasses.icon, config.iconColor)} />
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <span className={cn('font-medium', sizeClasses.text)}>
                {drift.fieldName}
              </span>
              <span className={cn(
                'inline-flex items-center rounded-full border px-2 py-0.5 text-xs font-medium',
                config.color
              )}>
                {config.label}
              </span>
            </div>
            
            <p className={cn('mt-1 opacity-90', sizeClasses.text)}>
              {drift.description}
            </p>
            
            {drift.suggestedAction && (
              <p className={cn('mt-2 opacity-75', sizeClasses.text)}>
                <strong>Suggestion:</strong> {drift.suggestedAction}
              </p>
            )}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div
      className={cn(
        'inline-flex items-center gap-1 rounded-full border font-medium transition-colors',
        config.color,
        sizeClasses.container,
        className
      )}
      data-testid={`drift-badge-${drift.severity.toLowerCase()}`}
      title={`${drift.driftType.replace('_', ' ')}: ${drift.description}`}
    >
      <Icon className={cn(sizeClasses.icon, config.iconColor)} />
      <span className={sizeClasses.text}>
        {drift.driftType.replace('_', ' ')}
      </span>
    </div>
  )
}

// Convenience components for different severities
export function CriticalDriftBadge(props: Omit<DriftBadgeProps, 'drift'> & { fieldName: string; description: string }) {
  const drift: DriftIndicator = {
    fieldName: props.fieldName,
    driftType: 'TYPE_MISMATCH',
    severity: 'CRITICAL',
    description: props.description,
    suggestedAction: 'Review and fix immediately'
  }
  return <DriftBadge {...props} drift={drift} />
}

export function HighDriftBadge(props: Omit<DriftBadgeProps, 'drift'> & { fieldName: string; description: string }) {
  const drift: DriftIndicator = {
    fieldName: props.fieldName,
    driftType: 'MISSING_FIELD',
    severity: 'HIGH',
    description: props.description,
    suggestedAction: 'Address soon'
  }
  return <DriftBadge {...props} drift={drift} />
}

export function MediumDriftBadge(props: Omit<DriftBadgeProps, 'drift'> & { fieldName: string; description: string }) {
  const drift: DriftIndicator = {
    fieldName: props.fieldName,
    driftType: 'EXTRA_FIELD',
    severity: 'MEDIUM',
    description: props.description,
    suggestedAction: 'Consider addressing'
  }
  return <DriftBadge {...props} drift={drift} />
}

export function LowDriftBadge(props: Omit<DriftBadgeProps, 'drift'> & { fieldName: string; description: string }) {
  const drift: DriftIndicator = {
    fieldName: props.fieldName,
    driftType: 'REQUIRED_MISMATCH',
    severity: 'LOW',
    description: props.description,
    suggestedAction: 'Monitor'
  }
  return <DriftBadge {...props} drift={drift} />
}
