'use client'

import { useState, useEffect } from 'react'
import { QueryOperation, QueryHelperResponse } from '@/lib/types'
import { cn, copyToClipboard } from '@/lib/utils'
import { Copy, Check, ChevronDown } from 'lucide-react'
import { CodeSnippet } from './CodeSnippet'

interface QueryHelperProps {
  typeName: string
  fieldPath: string
  onOperationChange: (operation: QueryOperation) => void
  className?: string
  initialOperation?: QueryOperation
}

const operations: { value: QueryOperation; label: string; description: string }[] = [
  {
    value: 'FIND',
    label: 'Find',
    description: 'Query documents matching criteria'
  },
  {
    value: 'INSERT',
    label: 'Insert',
    description: 'Create new documents'
  },
  {
    value: 'UPDATE',
    label: 'Update',
    description: 'Modify existing documents'
  },
  {
    value: 'DELETE',
    label: 'Delete',
    description: 'Remove documents'
  },
  {
    value: 'AGGREGATE',
    label: 'Aggregate',
    description: 'Perform aggregation operations'
  }
]

export function QueryHelper({
  typeName,
  fieldPath,
  onOperationChange,
  className,
  initialOperation = 'FIND'
}: QueryHelperProps) {
  const [selectedOperation, setSelectedOperation] = useState<QueryOperation>(initialOperation)
  const [examples, setExamples] = useState<QueryHelperResponse | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [copiedMongo, setCopiedMongo] = useState(false)
  const [copiedCSharp, setCopiedCSharp] = useState(false)

  // Generate examples based on operation and field
  const generateExamples = (operation: QueryOperation, type: string, field: string): QueryHelperResponse => {
    const fieldParts = field.split('.')
    const fieldName = fieldParts[fieldParts.length - 1]
    const nestedPath = fieldParts.length > 1 ? fieldParts.slice(0, -1).join('.') : ''

    const generateMongoShell = () => {
      switch (operation) {
        case 'FIND':
          return `db.${type.toLowerCase()}s.find({ ${field}: { $exists: true } })`
        case 'INSERT':
          return `db.${type.toLowerCase()}s.insertOne({ ${field}: "value" })`
        case 'UPDATE':
          return `db.${type.toLowerCase()}s.updateOne({ _id: ObjectId("...") }, { $set: { ${field}: "newValue" } })`
        case 'DELETE':
          return `db.${type.toLowerCase()}s.deleteOne({ ${field}: "value" })`
        case 'AGGREGATE':
          return `db.${type.toLowerCase()}s.aggregate([{ $match: { ${field}: { $exists: true } } }])`
        default:
          return `// ${operation} operation for ${field}`
      }
    }

    const generateCSharpBuilder = () => {
      const builderType = `${type}Builder`
      switch (operation) {
        case 'FIND':
          return `var query = Builders<${type}>.Filter.Exists(x => x.${fieldName});\nvar result = await collection.Find(query).ToListAsync();`
        case 'INSERT':
          return `var document = new ${type} { ${fieldName} = "value" };\nawait collection.InsertOneAsync(document);`
        case 'UPDATE':
          return `var filter = Builders<${type}>.Filter.Eq(x => x.Id, id);\nvar update = Builders<${type}>.Update.Set(x => x.${fieldName}, "newValue");\nawait collection.UpdateOneAsync(filter, update);`
        case 'DELETE':
          return `var filter = Builders<${type}>.Filter.Eq(x => x.${fieldName}, "value");\nawait collection.DeleteOneAsync(filter);`
        case 'AGGREGATE':
          return `var pipeline = new BsonDocument[]\n{\n    new BsonDocument("$match", new BsonDocument("${field}", new BsonDocument("$exists", true)))\n};\nvar result = await collection.Aggregate(pipeline).ToListAsync();`
        default:
          return `// ${operation} operation for ${field}`
      }
    }

    return {
      mongoShell: generateMongoShell(),
      csharpBuilder: generateCSharpBuilder(),
      operation,
      fieldPath: field,
      typeName: type,
      examples: {
        mongoShell: generateMongoShell(),
        csharpBuilder: generateCSharpBuilder()
      }
    }
  }

  useEffect(() => {
    setIsLoading(true)
    setError(null)

    try {
      const examples = generateExamples(selectedOperation, typeName, fieldPath)
      setExamples(examples)
      onOperationChange(selectedOperation)
    } catch (err) {
      setError('Failed to generate examples')
    } finally {
      setIsLoading(false)
    }
  }, [selectedOperation, typeName, fieldPath, onOperationChange])

  const handleCopyMongo = async () => {
    if (examples?.mongoShell) {
      const success = await copyToClipboard(examples.mongoShell)
      if (success) {
        setCopiedMongo(true)
        setTimeout(() => setCopiedMongo(false), 2000)
      }
    }
  }

  const handleCopyCSharp = async () => {
    if (examples?.csharpBuilder) {
      const success = await copyToClipboard(examples.csharpBuilder)
      if (success) {
        setCopiedCSharp(true)
        setTimeout(() => setCopiedCSharp(false), 2000)
      }
    }
  }

  return (
    <div className={cn('space-y-4', className)} data-testid="query-helper-panel">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-sm font-medium text-foreground">Query Helper</h3>
          <p className="text-xs text-muted-foreground mt-1">
            Generate code examples for <code className="text-xs bg-muted px-1 py-0.5 rounded">{typeName}.{fieldPath}</code>
          </p>
        </div>
      </div>

      {/* Operation Selector */}
      <div className="space-y-2">
        <label className="text-sm font-medium text-foreground">Operation</label>
        <select
          value={selectedOperation}
          onChange={(e) => setSelectedOperation(e.target.value as QueryOperation)}
          className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
          data-testid="operation-selector"
        >
          {operations.map((op) => (
            <option key={op.value} value={op.value}>
              {op.label} - {op.description}
            </option>
          ))}
        </select>
      </div>

      {/* Examples */}
      {isLoading ? (
        <div className="flex items-center justify-center py-8">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <div className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
            Generating examples...
          </div>
        </div>
      ) : error ? (
        <div className="text-center py-8">
          <div className="text-sm text-muted-foreground">{error}</div>
        </div>
      ) : examples ? (
        <div className="space-y-4">
          {/* MongoDB Shell Example */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-sm font-medium text-foreground">MongoDB Shell</label>
              <button
                onClick={handleCopyMongo}
                className={cn(
                  'flex items-center gap-1 px-2 py-1 text-xs font-medium rounded transition-colors',
                  'hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
                  copiedMongo 
                    ? 'text-green-600 bg-green-50' 
                    : 'text-muted-foreground hover:text-foreground'
                )}
                data-testid="copy-mongo-button"
              >
                {copiedMongo ? (
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
            <CodeSnippet
              code={examples.mongoShell}
              language="javascript"
              height={120}
              data-testid="mongo-shell-example"
            />
          </div>

          {/* C# Builder Example */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-sm font-medium text-foreground">C# Builders&lt;T&gt;</label>
              <button
                onClick={handleCopyCSharp}
                className={cn(
                  'flex items-center gap-1 px-2 py-1 text-xs font-medium rounded transition-colors',
                  'hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2',
                  copiedCSharp 
                    ? 'text-green-600 bg-green-50' 
                    : 'text-muted-foreground hover:text-foreground'
                )}
                data-testid="copy-csharp-button"
              >
                {copiedCSharp ? (
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
            <CodeSnippet
              code={examples.csharpBuilder}
              language="csharp"
              height={120}
              data-testid="csharp-builder-example"
            />
          </div>
        </div>
      ) : null}

      {/* Field Path Display */}
      <div className="pt-4 border-t border-border">
        <div className="text-xs text-muted-foreground">
          <span className="font-medium">Field Path:</span> {fieldPath}
        </div>
        <div className="text-xs text-muted-foreground mt-1">
          <span className="font-medium">Type:</span> {typeName}
        </div>
        <div className="text-xs text-muted-foreground mt-1">
          <span className="font-medium">Operation:</span> {selectedOperation}
        </div>
      </div>
    </div>
  )
}
