using Microsoft.Extensions.Logging;
using Cataloger.Scanner.Models;
using System.Text.RegularExpressions;

namespace Cataloger.Scanner.Samplers;

/// <summary>
/// Service for detecting PII in sampled data.
/// </summary>
public class PIIDetector : IPIIDetector
{
    private readonly ILogger<PIIDetector> _logger;
    private readonly Dictionary<string, PIIDetectionRule> _detectionRules;

    public PIIDetector(ILogger<PIIDetector> logger)
    {
        _logger = logger;
        _detectionRules = InitializeDetectionRules();
    }

    /// <summary>
    /// Detects PII in a field based on field name and sample values.
    /// </summary>
    /// <param name="fieldName">Name of the field to analyze.</param>
    /// <param name="sampleValues">Sample values from the field.</param>
    /// <returns>PII detection result if PII is found, null otherwise.</returns>
    public async Task<PIIDetection?> DetectPIIAsync(string fieldName, List<object> sampleValues)
    {
        try
        {
            // Check field name patterns
            var fieldNameDetection = DetectPIIByFieldName(fieldName);
            if (fieldNameDetection != null)
            {
                return fieldNameDetection;
            }

            // Check value patterns
            var valueDetection = await DetectPIIByValuesAsync(fieldName, sampleValues);
            if (valueDetection != null)
            {
                return valueDetection;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect PII for field {FieldName}", fieldName);
            return null;
        }
    }

    private PIIDetection? DetectPIIByFieldName(string fieldName)
    {
        var lowerFieldName = fieldName.ToLowerInvariant();

        foreach (var rule in _detectionRules.Values)
        {
            if (rule.FieldNamePatterns.Any(pattern => Regex.IsMatch(lowerFieldName, pattern, RegexOptions.IgnoreCase)))
            {
                return new PIIDetection
                {
                    FieldName = fieldName,
                    PIIType = rule.PIIType,
                    DetectionMethod = "field_name",
                    Confidence = rule.Confidence,
                    IsRedacted = true,
                    InstanceCount = 1,
                    RequiresManualReview = rule.RequiresManualReview,
                    Metadata = new Dictionary<string, object>
                    {
                        ["matched_pattern"] = rule.FieldNamePatterns.First(pattern => Regex.IsMatch(lowerFieldName, pattern, RegexOptions.IgnoreCase))
                    }
                };
            }
        }

        return null;
    }

    private async Task<PIIDetection?> DetectPIIByValuesAsync(string fieldName, List<object> sampleValues)
    {
        var stringValues = sampleValues
            .Where(v => v is string)
            .Cast<string>()
            .ToList();

        if (stringValues.Count == 0)
        {
            return null;
        }

        foreach (var rule in _detectionRules.Values)
        {
            if (rule.ValuePatterns.Count == 0)
            {
                continue;
            }

            var matchingValues = stringValues
                .Where(v => rule.ValuePatterns.Any(pattern => Regex.IsMatch(v, pattern)))
                .ToList();

            if (matchingValues.Count > 0)
            {
                var confidence = CalculateValueConfidence(matchingValues.Count, stringValues.Count, rule.Confidence);

                return new PIIDetection
                {
                    FieldName = fieldName,
                    PIIType = rule.PIIType,
                    DetectionMethod = "value_pattern",
                    Confidence = confidence,
                    IsRedacted = true,
                    InstanceCount = matchingValues.Count,
                    RequiresManualReview = rule.RequiresManualReview,
                    Metadata = new Dictionary<string, object>
                    {
                        ["matching_values_count"] = matchingValues.Count,
                        ["total_values_count"] = stringValues.Count,
                        ["matched_patterns"] = rule.ValuePatterns.Where(pattern => 
                            stringValues.Any(v => Regex.IsMatch(v, pattern))).ToList()
                    }
                };
            }
        }

        return null;
    }

    private double CalculateValueConfidence(int matchingCount, int totalCount, double baseConfidence)
    {
        var ratio = (double)matchingCount / totalCount;
        return Math.Min(baseConfidence * ratio, 1.0);
    }

    private Dictionary<string, PIIDetectionRule> InitializeDetectionRules()
    {
        return new Dictionary<string, PIIDetectionRule>
        {
            ["email"] = new PIIDetectionRule
            {
                PIIType = "email",
                FieldNamePatterns = new[] { @"email", @"e-mail", @"mail" },
                ValuePatterns = new[] { @"^[^@\s]+@[^@\s]+\.[^@\s]+$" },
                Confidence = 0.95,
                RequiresManualReview = false
            },
            ["phone"] = new PIIDetectionRule
            {
                PIIType = "phone",
                FieldNamePatterns = new[] { @"phone", @"telephone", @"mobile", @"cell" },
                ValuePatterns = new[] { @"^\+?[\d\s\-\(\)]+$", @"^\d{3}-\d{3}-\d{4}$", @"^\(\d{3}\)\s?\d{3}-\d{4}$" },
                Confidence = 0.85,
                RequiresManualReview = false
            },
            ["ssn"] = new PIIDetectionRule
            {
                PIIType = "ssn",
                FieldNamePatterns = new[] { @"ssn", @"social", @"security" },
                ValuePatterns = new[] { @"^\d{3}-\d{2}-\d{4}$", @"^\d{9}$" },
                Confidence = 0.98,
                RequiresManualReview = true
            },
            ["credit_card"] = new PIIDetectionRule
            {
                PIIType = "credit_card",
                FieldNamePatterns = new[] { @"credit", @"card", @"payment", @"billing" },
                ValuePatterns = new[] { @"^\d{4}[\s\-]?\d{4}[\s\-]?\d{4}[\s\-]?\d{4}$" },
                Confidence = 0.90,
                RequiresManualReview = true
            },
            ["address"] = new PIIDetectionRule
            {
                PIIType = "address",
                FieldNamePatterns = new[] { @"address", @"street", @"city", @"zip", @"postal" },
                ValuePatterns = new[] { @"\d+\s+[A-Za-z\s]+(?:Street|St|Avenue|Ave|Road|Rd|Drive|Dr|Lane|Ln|Boulevard|Blvd)" },
                Confidence = 0.70,
                RequiresManualReview = false
            },
            ["name"] = new PIIDetectionRule
            {
                PIIType = "name",
                FieldNamePatterns = new[] { @"name", @"firstname", @"lastname", @"fullname" },
                ValuePatterns = new[] { @"^[A-Za-z\s]+$" },
                Confidence = 0.60,
                RequiresManualReview = true
            },
            ["ip_address"] = new PIIDetectionRule
            {
                PIIType = "ip_address",
                FieldNamePatterns = new[] { @"ip", @"address" },
                ValuePatterns = new[] { @"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", @"^(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$" },
                Confidence = 0.95,
                RequiresManualReview = false
            },
            ["jwt"] = new PIIDetectionRule
            {
                PIIType = "jwt",
                FieldNamePatterns = new[] { @"token", @"jwt", @"bearer" },
                ValuePatterns = new[] { @"^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]*$" },
                Confidence = 0.90,
                RequiresManualReview = false
            },
            ["api_key"] = new PIIDetectionRule
            {
                PIIType = "api_key",
                FieldNamePatterns = new[] { @"key", @"secret", @"password", @"passwd" },
                ValuePatterns = new[] { @"^[A-Za-z0-9]{20,}$" },
                Confidence = 0.80,
                RequiresManualReview = true
            },
            ["uuid"] = new PIIDetectionRule
            {
                PIIType = "uuid",
                FieldNamePatterns = new[] { @"id", @"uuid", @"guid" },
                ValuePatterns = new[] { @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$" },
                Confidence = 0.95,
                RequiresManualReview = false
            }
        };
    }

    private class PIIDetectionRule
    {
        public string PIIType { get; set; } = string.Empty;
        public string[] FieldNamePatterns { get; set; } = Array.Empty<string>();
        public string[] ValuePatterns { get; set; } = Array.Empty<string>();
        public double Confidence { get; set; }
        public bool RequiresManualReview { get; set; }
    }
}
