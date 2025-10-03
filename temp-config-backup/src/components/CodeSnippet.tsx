'use client'

import { useState, useRef } from 'react'
import { Editor } from '@monaco-editor/react'
import { Copy, Check } from 'lucide-react'
import { cn, copyToClipboard } from '@/lib/utils'

interface CodeSnippetProps {
  code: string
  language: string
  readOnly?: boolean
  onCopy?: () => void
  className?: string
  height?: string | number
  showLineNumbers?: boolean
  theme?: 'light' | 'dark'
}

export function CodeSnippet({
  code,
  language,
  readOnly = true,
  onCopy,
  className,
  height = '200px',
  showLineNumbers = true,
  theme = 'light'
}: CodeSnippetProps) {
  const [copied, setCopied] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const editorRef = useRef<any>(null)

  const handleCopy = async () => {
    const success = await copyToClipboard(code)
    if (success) {
      setCopied(true)
      onCopy?.()
      setTimeout(() => setCopied(false), 2000)
    }
  }

  const handleEditorDidMount = (editor: any) => {
    editorRef.current = editor
    setIsLoading(false)
    
    // Configure editor options
    editor.updateOptions({
      readOnly,
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      fontSize: 14,
      lineHeight: 20,
      padding: { top: 16, bottom: 16 },
      wordWrap: 'on' as const,
      automaticLayout: true
    })

    // Add keyboard shortcut for copy
    editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyC, () => {
      if (readOnly) {
        handleCopy()
      }
    })
  }

  const getLanguageId = (lang: string) => {
    switch (lang.toLowerCase()) {
      case 'javascript':
        return 'javascript'
      case 'typescript':
        return 'typescript'
      case 'csharp':
      case 'c#':
        return 'csharp'
      case 'python':
        return 'python'
      case 'java':
        return 'java'
      case 'json':
        return 'json'
      case 'yaml':
      case 'yml':
        return 'yaml'
      case 'sql':
        return 'sql'
      case 'shell':
      case 'bash':
        return 'shell'
      case 'powershell':
        return 'powershell'
      default:
        return 'plaintext'
    }
  }

  return (
    <div className={cn('relative rounded-lg border border-border overflow-hidden', className)}>
      {/* Header */}
      <div className="flex items-center justify-between bg-muted/50 px-4 py-2 border-b border-border">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-foreground">
            {language.toUpperCase()}
          </span>
          {isLoading && (
            <div className="h-2 w-2 rounded-full bg-primary animate-pulse" />
          )}
        </div>
        
        <button
          onClick={handleCopy}
          className={cn(
            'flex items-center gap-1 px-2 py-1 text-xs font-medium rounded transition-colors',
            'hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
            copied 
              ? 'text-green-600 bg-green-50' 
              : 'text-muted-foreground hover:text-foreground'
          )}
          data-testid="copy-code-button"
          title="Copy to clipboard"
        >
          {copied ? (
            <>
              <Check className="h-3 w-3" />
              Copied
            </>
          ) : (
            <>
              <Copy className="h-3 w-3" />
              Copy
            </>
          )}
        </button>
      </div>

      {/* Editor */}
      <div className="relative">
        <Editor
          height={height}
          language={getLanguageId(language)}
          value={code}
          onMount={handleEditorDidMount}
          theme={theme === 'dark' ? 'vs-dark' : 'light'}
          options={{
            readOnly,
            lineNumbers: showLineNumbers ? 'on' : 'off',
            minimap: { enabled: false },
            scrollBeyondLastLine: false,
            fontSize: 14,
            lineHeight: 20,
            padding: { top: 16, bottom: 16 },
            wordWrap: 'on',
            automaticLayout: true,
            contextmenu: false,
            selectOnLineNumbers: true,
            roundedSelection: false,
            cursorStyle: readOnly ? 'line' : 'line',
            cursorBlinking: readOnly ? 'solid' : 'blink',
            renderLineHighlight: 'none',
            hideCursorInOverviewRuler: true,
            overviewRulerBorder: false,
            scrollbar: {
              vertical: 'auto',
              horizontal: 'auto',
              verticalScrollbarSize: 8,
              horizontalScrollbarSize: 8
            }
          }}
          data-testid="code-snippet"
        />
        
        {/* Loading overlay */}
        {isLoading && (
          <div className="absolute inset-0 bg-background/50 flex items-center justify-center">
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <div className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
              Loading editor...
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

// Specialized components for common use cases
export function MongoShellSnippet({ code, ...props }: Omit<CodeSnippetProps, 'language'>) {
  return <CodeSnippet {...props} code={code} language="javascript" />
}

export function CSharpBuilderSnippet({ code, ...props }: Omit<CodeSnippetProps, 'language'>) {
  return <CodeSnippet {...props} code={code} language="csharp" />
}

export function JsonSnippet({ code, ...props }: Omit<CodeSnippetProps, 'language'>) {
  return <CodeSnippet {...props} code={code} language="json" />
}

export function SqlSnippet({ code, ...props }: Omit<CodeSnippetProps, 'language'>) {
  return <CodeSnippet {...props} code={code} language="sql" />
}
