'use client'

import { useEffect, useCallback } from 'react'

interface KeyboardShortcut {
  key: string
  ctrlKey?: boolean
  metaKey?: boolean
  shiftKey?: boolean
  altKey?: boolean
  callback: () => void
  preventDefault?: boolean
}

interface UseKeyboardOptions {
  enabled?: boolean
  target?: HTMLElement | Document
}

export function useKeyboard(
  shortcuts: KeyboardShortcut[],
  options: UseKeyboardOptions = {}
) {
  const { enabled = true, target = document } = options

  const handleKeyDown = useCallback((event: KeyboardEvent) => {
    if (!enabled) return

    for (const shortcut of shortcuts) {
      const {
        key,
        ctrlKey = false,
        metaKey = false,
        shiftKey = false,
        altKey = false,
        callback,
        preventDefault = true
      } = shortcut

      // Check if the key combination matches
      if (
        event.key.toLowerCase() === key.toLowerCase() &&
        event.ctrlKey === ctrlKey &&
        event.metaKey === metaKey &&
        event.shiftKey === shiftKey &&
        event.altKey === altKey
      ) {
        if (preventDefault) {
          event.preventDefault()
        }
        callback()
        break // Only execute the first matching shortcut
      }
    }
  }, [shortcuts, enabled])

  useEffect(() => {
    if (!enabled) return

    target.addEventListener('keydown', handleKeyDown)
    return () => target.removeEventListener('keydown', handleKeyDown)
  }, [handleKeyDown, enabled, target])
}

// Common keyboard shortcuts
export const COMMON_SHORTCUTS = {
  SEARCH: {
    key: 'k',
    metaKey: true,
    callback: () => {
      // Focus search input
      const searchInput = document.querySelector('[data-testid="search-input"]') as HTMLInputElement
      if (searchInput) {
        searchInput.focus()
      }
    }
  },
  ESCAPE: {
    key: 'Escape',
    callback: () => {
      // Close modals, clear search, etc.
      const activeElement = document.activeElement as HTMLElement
      if (activeElement && activeElement.blur) {
        activeElement.blur()
      }
    }
  },
  COPY: {
    key: 'c',
    metaKey: true,
    callback: () => {
      // Copy selected text or current code snippet
      if (window.getSelection()?.toString()) {
        navigator.clipboard.writeText(window.getSelection()?.toString() || '')
      }
    }
  }
} as const

// Hook for common shortcuts
export function useCommonShortcuts() {
  useKeyboard([
    COMMON_SHORTCUTS.SEARCH,
    COMMON_SHORTCUTS.ESCAPE,
    COMMON_SHORTCUTS.COPY
  ])
}

// Hook for search-specific shortcuts
export function useSearchShortcuts(
  onClear: () => void,
  onFocus: () => void
) {
  useKeyboard([
    {
      key: 'k',
      metaKey: true,
      callback: onFocus
    },
    {
      key: 'Escape',
      callback: onClear
    }
  ])
}

// Hook for navigation shortcuts
export function useNavigationShortcuts(
  onPrevious: () => void,
  onNext: () => void
) {
  useKeyboard([
    {
      key: 'ArrowLeft',
      callback: onPrevious
    },
    {
      key: 'ArrowRight',
      callback: onNext
    }
  ])
}

// Hook for modal shortcuts
export function useModalShortcuts(
  onClose: () => void,
  onConfirm?: () => void
) {
  useKeyboard([
    {
      key: 'Escape',
      callback: onClose
    },
    ...(onConfirm ? [{
      key: 'Enter',
      callback: onConfirm
    }] : [])
  ])
}
