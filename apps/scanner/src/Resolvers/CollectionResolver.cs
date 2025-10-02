using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Text.RegularExpressions;

namespace Cataloger.Scanner.Resolvers;

/// <summary>
/// Service for resolving MongoDB collection names from C# code using various resolution methods.
/// </summary>
public class CollectionResolver
{
    private readonly ILogger<CollectionResolver> _logger;

    public CollectionResolver(ILogger<CollectionResolver> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Resolves collection names for a given code type.
    /// </summary>
    /// <param name="codeType">The code type to resolve collection names for.</param>
    /// <param name="syntaxTree">The C# syntax tree to analyze.</param>
    /// <param name="semanticModel">The semantic model for the syntax tree.</param>
    /// <param name="provenance">Provenance information for the resolution.</param>
    /// <returns>List of resolved collection mappings.</returns>
    public async Task<List<CollectionMapping>> ResolveCollectionNamesAsync(
        CodeType codeType,
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        ProvenanceRecord provenance)
    {
        var mappings = new List<CollectionMapping>();
        var root = await syntaxTree.GetRootAsync();

        _logger.LogDebug("Resolving collection names for {TypeName} in {FilePath}", 
            codeType.Name, provenance.FilePath);

        // Find GetCollection<T> calls
        var getCollectionCalls = root.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(IsGetCollectionCall);

        foreach (var call in getCollectionCalls)
        {
            try
            {
                var mapping = await ResolveCollectionFromCallAsync(call, semanticModel, codeType, provenance);
                if (mapping != null)
                {
                    mappings.Add(mapping);
                    _logger.LogDebug("Resolved collection mapping: {CollectionName} for {TypeName}", 
                        mapping.CollectionName, codeType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve collection from call in {FilePath}", 
                    provenance.FilePath);
            }
        }

        // Find IMongoCollection<T> field declarations
        var collectionFields = root.DescendantNodes()
            .OfType<FieldDeclarationSyntax>()
            .Where(f => IsMongoCollectionField(f, semanticModel));

        foreach (var field in collectionFields)
        {
            try
            {
                var mapping = await ResolveCollectionFromFieldAsync(field, semanticModel, codeType, provenance);
                if (mapping != null)
                {
                    mappings.Add(mapping);
                    _logger.LogDebug("Resolved collection mapping from field: {CollectionName} for {TypeName}", 
                        mapping.CollectionName, codeType.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to resolve collection from field in {FilePath}", 
                    provenance.FilePath);
            }
        }

        // If no explicit mappings found, try to infer from naming conventions
        if (mappings.Count == 0)
        {
            var inferredMapping = InferCollectionName(codeType, provenance);
            if (inferredMapping != null)
            {
                mappings.Add(inferredMapping);
                _logger.LogDebug("Inferred collection mapping: {CollectionName} for {TypeName}", 
                    inferredMapping.CollectionName, codeType.Name);
            }
        }

        _logger.LogInformation("Resolved {Count} collection mappings for {TypeName} in {FilePath}", 
            mappings.Count, codeType.Name, provenance.FilePath);

        return mappings;
    }

    private bool IsGetCollectionCall(InvocationExpressionSyntax invocation)
    {
        var memberAccess = invocation.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null) return false;

        return memberAccess.Name.Identifier.ValueText == "GetCollection";
    }

    private bool IsMongoCollectionField(FieldDeclarationSyntax field, SemanticModel semanticModel)
    {
        var type = semanticModel.GetTypeInfo(field.Declaration.Type).Type;
        if (type == null) return false;

        var typeName = type.ToDisplayString();
        return typeName.Contains("IMongoCollection") || typeName.Contains("MongoCollection");
    }

    private async Task<CollectionMapping?> ResolveCollectionFromCallAsync(
        InvocationExpressionSyntax call,
        SemanticModel semanticModel,
        CodeType codeType,
        ProvenanceRecord provenance)
    {
        var memberAccess = call.Expression as MemberAccessExpressionSyntax;
        if (memberAccess == null) return null;

        // Check if this is a generic call with the correct type
        if (call.ArgumentList.Arguments.Count == 0) return null;

        var firstArg = call.ArgumentList.Arguments[0];
        var collectionName = await ResolveStringValueAsync(firstArg.Expression, semanticModel);

        if (string.IsNullOrEmpty(collectionName)) return null;

        // Determine resolution method and confidence
        var (method, confidence) = DetermineResolutionMethod(firstArg.Expression, semanticModel);

        return new CollectionMapping
        {
            TypeId = codeType.Id,
            CollectionName = collectionName,
            ResolutionMethod = method,
            Confidence = confidence,
            ResolutionContext = firstArg.Expression.ToString(),
            IsPrimary = true,
            Provenance = provenance
        };
    }

    private async Task<CollectionMapping?> ResolveCollectionFromFieldAsync(
        FieldDeclarationSyntax field,
        SemanticModel semanticModel,
        CodeType codeType,
        ProvenanceRecord provenance)
    {
        // Look for field initialization or assignment
        var variable = field.Declaration.Variables.FirstOrDefault();
        if (variable == null) return null;

        // Find assignments to this field
        var root = await field.SyntaxTree.GetRootAsync();
        var assignments = root.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(a => a.Left.ToString().Contains(variable.Identifier.ValueText));

        foreach (var assignment in assignments)
        {
            if (assignment.Right is InvocationExpressionSyntax invocation)
            {
                var mapping = await ResolveCollectionFromCallAsync(invocation, semanticModel, codeType, provenance);
                if (mapping != null)
                {
                    return mapping;
                }
            }
        }

        return null;
    }

    private async Task<string?> ResolveStringValueAsync(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        // Handle string literals
        if (expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        // Handle identifier names (constants, readonly fields)
        if (expression is IdentifierNameSyntax identifier)
        {
            var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol != null)
            {
                return await ResolveSymbolValueAsync(symbol, semanticModel);
            }
        }

        // Handle member access (e.g., Constants.CollectionName)
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (symbol != null)
            {
                return await ResolveSymbolValueAsync(symbol, semanticModel);
            }
        }

        // Handle string concatenation
        if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.AddExpression))
        {
            var left = await ResolveStringValueAsync(binary.Left, semanticModel);
            var right = await ResolveStringValueAsync(binary.Right, semanticModel);
            if (left != null && right != null)
            {
                return left + right;
            }
        }

        return null;
    }

    private async Task<string?> ResolveSymbolValueAsync(ISymbol symbol, SemanticModel semanticModel)
    {
        // Handle constants
        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.IsConst)
        {
            if (fieldSymbol.ConstantValue is string stringValue)
            {
                return stringValue;
            }
        }

        // Handle readonly fields with initializers
        if (symbol is IFieldSymbol readonlyField && readonlyField.IsReadOnly)
        {
            // Try to find the field declaration and extract the initializer
            var declarations = readonlyField.DeclaringSyntaxReferences;
            foreach (var declaration in declarations)
            {
                var syntax = await declaration.GetSyntaxAsync();
                if (syntax is VariableDeclaratorSyntax variableDeclarator)
                {
                    if (variableDeclarator.Initializer?.Value is LiteralExpressionSyntax literal)
                    {
                        return literal.Token.ValueText;
                    }
                }
            }
        }

        // Handle properties with getters
        if (symbol is IPropertySymbol propertySymbol)
        {
            // This would require more complex analysis to resolve property values
            // For now, return null to indicate we can't resolve it
            return null;
        }

        return null;
    }

    private (ResolutionMethod method, double confidence) DetermineResolutionMethod(
        ExpressionSyntax expression, 
        SemanticModel semanticModel)
    {
        // String literal - highest confidence
        if (expression is LiteralExpressionSyntax)
        {
            return (ResolutionMethod.Literal, 1.0);
        }

        // Constant - high confidence
        if (expression is IdentifierNameSyntax identifier)
        {
            var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
            if (symbol is IFieldSymbol field && field.IsConst)
            {
                return (ResolutionMethod.Constant, 0.9);
            }
        }

        // Member access to constant - high confidence
        if (expression is MemberAccessExpressionSyntax memberAccess)
        {
            var symbol = semanticModel.GetSymbolInfo(memberAccess).Symbol;
            if (symbol is IFieldSymbol field && field.IsConst)
            {
                return (ResolutionMethod.Constant, 0.9);
            }
        }

        // Readonly field - medium confidence
        if (expression is IdentifierNameSyntax readonlyIdentifier)
        {
            var symbol = semanticModel.GetSymbolInfo(readonlyIdentifier).Symbol;
            if (symbol is IFieldSymbol field && field.IsReadOnly)
            {
                return (ResolutionMethod.Constant, 0.7);
            }
        }

        // Configuration value - medium confidence
        if (IsConfigurationAccess(expression))
        {
            return (ResolutionMethod.Config, 0.6);
        }

        // Environment variable - low confidence
        if (IsEnvironmentVariableAccess(expression))
        {
            return (ResolutionMethod.Environment, 0.5);
        }

        // Unknown - very low confidence
        return (ResolutionMethod.Unknown, 0.1);
    }

    private bool IsConfigurationAccess(ExpressionSyntax expression)
    {
        var expressionText = expression.ToString();
        return expressionText.Contains("Configuration") ||
               expressionText.Contains("Settings") ||
               expressionText.Contains("Options");
    }

    private bool IsEnvironmentVariableAccess(ExpressionSyntax expression)
    {
        var expressionText = expression.ToString();
        return expressionText.Contains("Environment") ||
               expressionText.Contains("GetEnvironmentVariable");
    }

    private CollectionMapping? InferCollectionName(CodeType codeType, ProvenanceRecord provenance)
    {
        // Apply naming conventions to infer collection name
        var collectionName = ApplyNamingConvention(codeType.Name);
        
        if (string.IsNullOrEmpty(collectionName))
        {
            return null;
        }

        return new CollectionMapping
        {
            TypeId = codeType.Id,
            CollectionName = collectionName,
            ResolutionMethod = ResolutionMethod.Inferred,
            Confidence = 0.3, // Low confidence for inferred names
            ResolutionContext = $"Inferred from type name '{codeType.Name}' using naming convention",
            IsPrimary = false,
            Provenance = provenance
        };
    }

    private string ApplyNamingConvention(string typeName)
    {
        // Convert PascalCase to camelCase and pluralize
        if (string.IsNullOrEmpty(typeName))
        {
            return string.Empty;
        }

        // Remove common suffixes
        var baseName = typeName;
        var suffixes = new[] { "Entity", "Model", "Document", "Record" };
        foreach (var suffix in suffixes)
        {
            if (baseName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName.Substring(0, baseName.Length - suffix.Length);
                break;
            }
        }

        // Convert to camelCase
        var camelCase = char.ToLowerInvariant(baseName[0]) + baseName.Substring(1);

        // Simple pluralization (add 's' for now)
        return camelCase + "s";
    }
}
