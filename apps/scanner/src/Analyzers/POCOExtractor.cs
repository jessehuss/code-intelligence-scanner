using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Text.RegularExpressions;

namespace Cataloger.Scanner.Analyzers;

/// <summary>
/// Service for extracting POCO classes and their BSON attributes from C# code using Roslyn.
/// </summary>
public class POCOExtractor
{
    private readonly ILogger<POCOExtractor> _logger;

    public POCOExtractor(ILogger<POCOExtractor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts POCO classes from a C# syntax tree.
    /// </summary>
    /// <param name="syntaxTree">The C# syntax tree to analyze.</param>
    /// <param name="semanticModel">The semantic model for the syntax tree.</param>
    /// <param name="provenance">Provenance information for the extraction.</param>
    /// <returns>List of extracted CodeType objects.</returns>
    public async Task<List<CodeType>> ExtractPOCOsAsync(
        SyntaxTree syntaxTree, 
        SemanticModel semanticModel, 
        ProvenanceRecord provenance)
    {
        var codeTypes = new List<CodeType>();
        var root = await syntaxTree.GetRootAsync();
        
        _logger.LogDebug("Extracting POCOs from {FilePath}", provenance.FilePath);

        // Find all class declarations
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        
        foreach (var classDecl in classDeclarations)
        {
            try
            {
                var codeType = await ExtractCodeTypeAsync(classDecl, semanticModel, provenance);
                if (codeType != null)
                {
                    codeTypes.Add(codeType);
                    _logger.LogDebug("Extracted POCO: {ClassName} from {FilePath}", 
                        codeType.Name, provenance.FilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract POCO from class {ClassName} in {FilePath}", 
                    classDecl.Identifier.ValueText, provenance.FilePath);
            }
        }

        _logger.LogInformation("Extracted {Count} POCOs from {FilePath}", 
            codeTypes.Count, provenance.FilePath);
        
        return codeTypes;
    }

    private async Task<CodeType?> ExtractCodeTypeAsync(
        ClassDeclarationSyntax classDecl, 
        SemanticModel semanticModel, 
        ProvenanceRecord provenance)
    {
        var symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol == null) return null;

        // Check if this class has BSON attributes or MongoDB-related characteristics
        if (!HasMongoDBCharacteristics(classDecl, semanticModel))
        {
            return null;
        }

        var codeType = new CodeType
        {
            Name = symbol.Name,
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            Assembly = symbol.ContainingAssembly.Name,
            Provenance = provenance
        };

        // Extract fields
        codeType.Fields = ExtractFields(classDecl, semanticModel);
        
        // Extract BSON attributes
        codeType.BSONAttributes = ExtractBSONAttributes(classDecl);
        
        // Extract nullability information
        codeType.Nullability = ExtractNullabilityInfo(classDecl, semanticModel);
        
        // Extract discriminators
        codeType.Discriminators = ExtractDiscriminators(classDecl);

        return codeType;
    }

    private bool HasMongoDBCharacteristics(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
    {
        // Check for BSON attributes
        if (HasBSONAttributes(classDecl))
        {
            return true;
        }

        // Check for MongoDB-related base classes or interfaces
        var symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol != null)
        {
            // Check base types
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (IsMongoDBRelatedType(baseType))
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }

            // Check interfaces
            foreach (var interfaceType in symbol.Interfaces)
            {
                if (IsMongoDBRelatedType(interfaceType))
                {
                    return true;
                }
            }
        }

        // Check for MongoDB-related field types
        var fields = classDecl.Members.OfType<PropertyDeclarationSyntax>()
            .Concat(classDecl.Members.OfType<FieldDeclarationSyntax>());
        
        foreach (var field in fields)
        {
            if (HasMongoDBFieldType(field, semanticModel))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasBSONAttributes(ClassDeclarationSyntax classDecl)
    {
        var attributes = classDecl.AttributeLists.SelectMany(al => al.Attributes);
        return attributes.Any(attr => IsBSONAttribute(attr));
    }

    private bool IsBSONAttribute(AttributeSyntax attribute)
    {
        var attributeName = attribute.Name.ToString();
        return attributeName.Contains("Bson") || 
               attributeName.Contains("MongoDB") ||
               attributeName.Contains("Collection");
    }

    private bool IsMongoDBRelatedType(ITypeSymbol type)
    {
        var typeName = type.ToDisplayString();
        return typeName.Contains("MongoDB") || 
               typeName.Contains("Bson") ||
               typeName.Contains("IMongoCollection") ||
               typeName.Contains("IMongoDatabase");
    }

    private bool HasMongoDBFieldType(MemberDeclarationSyntax member, SemanticModel semanticModel)
    {
        if (member is PropertyDeclarationSyntax prop)
        {
            var type = semanticModel.GetTypeInfo(prop.Type).Type;
            return type != null && IsMongoDBRelatedType(type);
        }
        
        if (member is FieldDeclarationSyntax field)
        {
            var type = semanticModel.GetTypeInfo(field.Declaration.Type).Type;
            return type != null && IsMongoDBRelatedType(type);
        }

        return false;
    }

    private List<FieldDefinition> ExtractFields(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
    {
        var fields = new List<FieldDefinition>();

        // Extract properties
        var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>();
        foreach (var prop in properties)
        {
            var field = ExtractFieldFromProperty(prop, semanticModel);
            if (field != null)
            {
                fields.Add(field);
            }
        }

        // Extract fields
        var fieldDeclarations = classDecl.Members.OfType<FieldDeclarationSyntax>();
        foreach (var fieldDecl in fieldDeclarations)
        {
            var extractedFields = ExtractFieldsFromDeclaration(fieldDecl, semanticModel);
            fields.AddRange(extractedFields);
        }

        return fields;
    }

    private FieldDefinition? ExtractFieldFromProperty(PropertyDeclarationSyntax prop, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(prop);
        if (symbol == null) return null;

        var field = new FieldDefinition
        {
            Name = symbol.Name,
            Type = symbol.Type.ToDisplayString(),
            IsNullable = IsNullableType(symbol.Type),
            IsRequired = prop.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.InitAccessorDeclaration)) == true
        };

        // Extract BSON attributes
        field.BSONAttributes = ExtractBSONAttributesFromMember(prop);

        // Extract documentation
        field.Documentation = ExtractDocumentation(prop);

        return field;
    }

    private List<FieldDefinition> ExtractFieldsFromDeclaration(FieldDeclarationSyntax fieldDecl, SemanticModel semanticModel)
    {
        var fields = new List<FieldDefinition>();
        var type = semanticModel.GetTypeInfo(fieldDecl.Declaration.Type).Type;

        foreach (var variable in fieldDecl.Declaration.Variables)
        {
            var field = new FieldDefinition
            {
                Name = variable.Identifier.ValueText,
                Type = type?.ToDisplayString() ?? fieldDecl.Declaration.Type.ToString(),
                IsNullable = type != null && IsNullableType(type)
            };

            // Extract BSON attributes
            field.BSONAttributes = ExtractBSONAttributesFromMember(fieldDecl);

            fields.Add(field);
        }

        return fields;
    }

    private bool IsNullableType(ITypeSymbol type)
    {
        return type.NullableAnnotation == NullableAnnotation.Annotated ||
               type.IsReferenceType;
    }

    private List<BSONAttribute> ExtractBSONAttributes(ClassDeclarationSyntax classDecl)
    {
        var attributes = new List<BSONAttribute>();
        var attributeLists = classDecl.AttributeLists;

        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (IsBSONAttribute(attribute))
                {
                    var bsonAttr = ExtractBSONAttribute(attribute);
                    if (bsonAttr != null)
                    {
                        attributes.Add(bsonAttr);
                    }
                }
            }
        }

        return attributes;
    }

    private List<BSONAttribute> ExtractBSONAttributesFromMember(MemberDeclarationSyntax member)
    {
        var attributes = new List<BSONAttribute>();
        var attributeLists = member.AttributeLists;

        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (IsBSONAttribute(attribute))
                {
                    var bsonAttr = ExtractBSONAttribute(attribute);
                    if (bsonAttr != null)
                    {
                        attributes.Add(bsonAttr);
                    }
                }
            }
        }

        return attributes;
    }

    private BSONAttribute? ExtractBSONAttribute(AttributeSyntax attribute)
    {
        var attributeName = attribute.Name.ToString();
        
        // Extract attribute name without namespace
        var name = attributeName.Split('.').Last();
        
        // Extract attribute value
        string value = "true"; // Default value
        if (attribute.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = attribute.ArgumentList.Arguments[0];
            value = firstArg.Expression.ToString().Trim('"');
        }

        return new BSONAttribute
        {
            Name = name,
            Value = value
        };
    }

    private Models.NullabilityInfo? ExtractNullabilityInfo(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(classDecl);
        if (symbol == null) return null;

        var nullabilityInfo = new Models.NullabilityInfo
        {
            Enabled = true, // Assume enabled for now
            Context = "enabled"
        };

        // Extract nullable and non-nullable fields
        var properties = classDecl.Members.OfType<PropertyDeclarationSyntax>();
        foreach (var prop in properties)
        {
            var propSymbol = semanticModel.GetDeclaredSymbol(prop);
            if (propSymbol != null)
            {
                if (IsNullableType(propSymbol.Type))
                {
                    nullabilityInfo.NullableFields.Add(propSymbol.Name);
                }
                else
                {
                    nullabilityInfo.NonNullableFields.Add(propSymbol.Name);
                }
            }
        }

        return nullabilityInfo;
    }

    private List<string> ExtractDiscriminators(ClassDeclarationSyntax classDecl)
    {
        var discriminators = new List<string>();

        // Look for BsonDiscriminator attributes
        var attributes = classDecl.AttributeLists.SelectMany(al => al.Attributes);
        foreach (var attr in attributes)
        {
            if (attr.Name.ToString().Contains("BsonDiscriminator"))
            {
                if (attr.ArgumentList?.Arguments.Count > 0)
                {
                    var discriminatorValue = attr.ArgumentList.Arguments[0].Expression.ToString().Trim('"');
                    discriminators.Add(discriminatorValue);
                }
            }
        }

        return discriminators;
    }

    private string? ExtractDocumentation(MemberDeclarationSyntax member)
    {
        // Extract XML documentation comments
        var trivia = member.GetLeadingTrivia();
        var docComments = trivia.Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                                           t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));
        
        if (docComments.Any())
        {
            var docText = string.Join(" ", docComments.Select(t => t.ToString()));
            // Clean up XML tags
            return Regex.Replace(docText, @"<[^>]+>", "").Trim();
        }

        return null;
    }
}
