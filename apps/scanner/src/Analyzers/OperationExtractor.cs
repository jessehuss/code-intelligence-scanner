using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Text.RegularExpressions;

namespace Cataloger.Scanner.Analyzers;

/// <summary>
/// Service for extracting MongoDB operations from C# code using Roslyn.
/// </summary>
public class OperationExtractor
{
    private readonly ILogger<OperationExtractor> _logger;

    public OperationExtractor(ILogger<OperationExtractor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts MongoDB operations from a C# syntax tree.
    /// </summary>
    /// <param name="syntaxTree">The C# syntax tree to analyze.</param>
    /// <param name="semanticModel">The semantic model for the syntax tree.</param>
    /// <param name="collectionMappings">Available collection mappings.</param>
    /// <param name="provenance">Provenance information for the extraction.</param>
    /// <returns>List of extracted QueryOperation objects.</returns>
    public async Task<List<QueryOperation>> ExtractOperationsAsync(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        List<CollectionMapping> collectionMappings,
        ProvenanceRecord provenance)
    {
        var operations = new List<QueryOperation>();
        var root = await syntaxTree.GetRootAsync();

        _logger.LogDebug("Extracting MongoDB operations from {FilePath}", provenance.FilePath);

        // Find MongoDB operation calls
        var operationCalls = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(IsMongoDBOperation);

        foreach (var call in operationCalls)
        {
            try
            {
                var operation = await ExtractOperationFromCallAsync(call, semanticModel, collectionMappings, provenance);
                if (operation != null)
                {
                    operations.Add(operation);
                    _logger.LogDebug("Extracted operation: {OperationType} from {FilePath}", 
                        operation.OperationType, provenance.FilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract operation from call in {FilePath}", 
                    provenance.FilePath);
            }
        }

        _logger.LogInformation("Extracted {Count} MongoDB operations from {FilePath}", 
            operations.Count, provenance.FilePath);

        return operations;
    }

    private bool IsMongoDBOperation(InvocationExpressionSyntax invocation)
    {
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null) return false;

        var methodName = memberAccess.Name.Identifier.ValueText;
        var mongoDBOperations = new[]
        {
            "Find", "FindOne", "FindOneAndUpdate", "FindOneAndReplace", "FindOneAndDelete",
            "InsertOne", "InsertMany", "ReplaceOne", "UpdateOne", "UpdateMany",
            "DeleteOne", "DeleteMany", "Aggregate", "Count", "Distinct"
        };

        return mongoDBOperations.Contains(methodName);
    }

    private async Task<QueryOperation?> ExtractOperationFromCallAsync(
        InvocationExpressionSyntax call,
        SemanticModel semanticModel,
        List<CollectionMapping> collectionMappings,
        ProvenanceRecord provenance)
    {
        var memberAccess = call.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null) return null;

        var operationType = DetermineOperationType(memberAccess.Name.Identifier.ValueText);
        if (operationType == null) return null;

        // Find the collection this operation is performed on
        var collectionId = FindCollectionId(call, semanticModel, collectionMappings);
        if (string.IsNullOrEmpty(collectionId)) return null;

        var operation = new QueryOperation
        {
            OperationType = operationType.Value,
            CollectionId = collectionId,
            Provenance = provenance
        };

        // Extract operation-specific details
        await ExtractOperationDetailsAsync(call, semanticModel, operation);

        return operation;
    }

    private OperationType? DetermineOperationType(string methodName)
    {
        return methodName switch
        {
            "Find" or "FindOne" => OperationType.Find,
            "InsertOne" or "InsertMany" => OperationType.Insert,
            "UpdateOne" or "UpdateMany" => OperationType.Update,
            "ReplaceOne" => OperationType.Replace,
            "DeleteOne" or "DeleteMany" => OperationType.Delete,
            "FindOneAndUpdate" => OperationType.FindOneAndUpdate,
            "FindOneAndReplace" => OperationType.FindOneAndReplace,
            "FindOneAndDelete" => OperationType.FindOneAndDelete,
            "Aggregate" => OperationType.Aggregate,
            "Count" => OperationType.Count,
            "Distinct" => OperationType.Distinct,
            _ => null
        };
    }

    private string? FindCollectionId(
        InvocationExpressionSyntax call,
        SemanticModel semanticModel,
        List<CollectionMapping> collectionMappings)
    {
        // Walk up the syntax tree to find the collection variable
        var current = call.Parent;
        while (current != null)
        {
            if (current is MemberAccessExpressionSyntax memberAccess)
            {
                var collectionName = ExtractCollectionNameFromMemberAccess(memberAccess, semanticModel);
                if (!string.IsNullOrEmpty(collectionName))
                {
                    var mapping = collectionMappings.FirstOrDefault(m => m.CollectionName == collectionName);
                    return mapping?.Id;
                }
            }

            current = current.Parent;
        }

        return null;
    }

    private string? ExtractCollectionNameFromMemberAccess(
        MemberAccessExpressionSyntax memberAccess,
        SemanticModel semanticModel)
    {
        // This is a simplified implementation
        // In a real implementation, you would need to trace back through variable assignments
        // to find where the collection was obtained from GetCollection<T>()
        
        var expression = memberAccess.Expression.ToString();
        
        // Look for common collection variable patterns
        if (expression.Contains("Collection") || expression.Contains("coll"))
        {
            // Try to resolve the actual collection name
            // This would require more complex analysis
            return "unknown"; // Placeholder
        }

        return null;
    }

    private async Task ExtractOperationDetailsAsync(
        InvocationExpressionSyntax call,
        SemanticModel semanticModel,
        QueryOperation operation)
    {
        var arguments = call.ArgumentList.Arguments;

        switch (operation.OperationType)
        {
            case OperationType.Find:
                await ExtractFindOperationDetailsAsync(arguments, semanticModel, operation);
                break;
            case OperationType.Update:
                await ExtractUpdateOperationDetailsAsync(arguments, semanticModel, operation);
                break;
            case OperationType.Aggregate:
                await ExtractAggregateOperationDetailsAsync(arguments, semanticModel, operation);
                break;
            case OperationType.Insert:
                await ExtractInsertOperationDetailsAsync(arguments, semanticModel, operation);
                break;
            case OperationType.Delete:
                await ExtractDeleteOperationDetailsAsync(arguments, semanticModel, operation);
                break;
        }
    }

    private async Task ExtractFindOperationDetailsAsync(
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        SemanticModel semanticModel,
        QueryOperation operation)
    {
        if (arguments.Count > 0)
        {
            // First argument is typically the filter
            var filterArg = arguments[0];
            operation.Filters = await ExtractFilterExpressionsAsync(filterArg.Expression, semanticModel);
        }

        if (arguments.Count > 1)
        {
            // Second argument might be options or projection
            var secondArg = arguments[1];
            if (IsProjectionExpression(secondArg.Expression))
            {
                operation.Projections = await ExtractProjectionExpressionsAsync(secondArg.Expression, semanticModel);
            }
        }

        // Look for method chaining (Sort, Limit, Skip)
        await ExtractChainedOperationsAsync(operation);
    }

    private async Task ExtractUpdateOperationDetailsAsync(
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        SemanticModel semanticModel,
        QueryOperation operation)
    {
        if (arguments.Count > 0)
        {
            // First argument is typically the filter
            var filterArg = arguments[0];
            operation.Filters = await ExtractFilterExpressionsAsync(filterArg.Expression, semanticModel);
        }

        if (arguments.Count > 1)
        {
            // Second argument is typically the update
            var updateArg = arguments[1];
            // Extract update expressions (this would be more complex in practice)
        }
    }

    private async Task ExtractAggregateOperationDetailsAsync(
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        SemanticModel semanticModel,
        QueryOperation operation)
    {
        if (arguments.Count > 0)
        {
            // First argument is typically the pipeline
            var pipelineArg = arguments[0];
            operation.AggregationPipeline = await ExtractAggregationPipelineAsync(pipelineArg.Expression, semanticModel);
        }
    }

    private async Task ExtractInsertOperationDetailsAsync(
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        SemanticModel semanticModel,
        QueryOperation operation)
    {
        // Insert operations typically don't have filters, but might have options
        // This would be extracted based on the specific insert method
    }

    private async Task ExtractDeleteOperationDetailsAsync(
        SeparatedSyntaxList<ArgumentSyntax> arguments,
        SemanticModel semanticModel,
        QueryOperation operation)
    {
        if (arguments.Count > 0)
        {
            // First argument is typically the filter
            var filterArg = arguments[0];
            operation.Filters = await ExtractFilterExpressionsAsync(filterArg.Expression, semanticModel);
        }
    }

    private async Task<List<FilterExpression>> ExtractFilterExpressionsAsync(
        ExpressionSyntax expression,
        SemanticModel semanticModel)
    {
        var filters = new List<FilterExpression>();

        // This is a simplified implementation
        // In practice, you would need to parse the filter expression tree
        // and extract individual filter conditions

        if (expression is InvocationExpressionSyntax invocation)
        {
            var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
            if (memberAccess != null)
            {
                var filter = new FilterExpression
                {
                    FieldPath = "unknown", // Would be extracted from the expression
                    Operator = memberAccess.Name.Identifier.ValueText,
                    Value = "unknown" // Would be extracted from arguments
                };
                filters.Add(filter);
            }
        }

        return filters;
    }

    private async Task<List<ProjectionExpression>> ExtractProjectionExpressionsAsync(
        ExpressionSyntax expression,
        SemanticModel semanticModel)
    {
        var projections = new List<ProjectionExpression>();

        // This is a simplified implementation
        // In practice, you would need to parse the projection expression
        // and extract individual field projections

        return projections;
    }

    private async Task<List<AggregationStage>> ExtractAggregationPipelineAsync(
        ExpressionSyntax expression,
        SemanticModel semanticModel)
    {
        var stages = new List<AggregationStage>();

        // This is a simplified implementation
        // In practice, you would need to parse the aggregation pipeline
        // and extract individual stages

        return stages;
    }

    private bool IsProjectionExpression(ExpressionSyntax expression)
    {
        // Simple heuristic to determine if an expression is a projection
        var expressionText = expression.ToString();
        return expressionText.Contains("Project") || expressionText.Contains("Select");
    }

    private async Task ExtractChainedOperationsAsync(QueryOperation operation)
    {
        // This would extract chained operations like Sort, Limit, Skip
        // In practice, you would need to analyze the method call chain
        // and extract these operations from the syntax tree
    }
}
