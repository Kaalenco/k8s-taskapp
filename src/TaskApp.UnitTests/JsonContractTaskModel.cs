using System.Text.Json;
using TaskApp.Application.Models;

namespace TaskApp.UnitTests;

/// <summary>
/// Verifies the JSON contract of <see cref="TaskModel"/> — the property names that the frontend
/// reads (GET response) and writes (PUT/POST request body).
///
/// How to extend when adding a new property to TaskModel:
///   Serialization   – add a row to <see cref="SerializationCases"/> with (jsonKey, expectedValue).
///   Deserialization – add a row to <see cref="DeserializationCases"/> with (jsonKey, expected C# value, m => m.Property).
/// </summary>
public class JsonContractTaskModel
{
    // ASP.NET Core serializes responses with camelCase by default.
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // Fixed model used as the source for all serialization cases.
    private static readonly TaskModel Source = new()
    {
        Id          = 42,
        Title       = "My Task",
        Description = "Some description",
        Status      = "in-progress",
    };

    // ── Serialization: TaskModel → JSON ──────────────────────────────────────
    //
    // Verifies that each C# property is serialized under the JSON key the frontend expects.
    // Add a row here for every new property: (json key, expected value from Source above).

    private static IEnumerable<TestCaseData> SerializationCases()
    {
        var testCases = new[]
        {
             ("id",          "42"              ),
             ("title",       "My Task"         ),
             ("description", "Some description"),
             ("status",      "in-progress"     ),
        };

        return testCases
            .Select(tc => new TestCaseData(tc.Item1, tc.Item2)
            .SetName($"Set property: {tc.Item1}"));      
    }

    [TestCaseSource(nameof(SerializationCases))]
    public void Serializes_ModelProperty_ToExpectedJsonKey(string jsonKey, string expectedValue)
    {
        var doc = JsonDocument.Parse(JsonSerializer.Serialize(Source, Options));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                    doc.RootElement.TryGetProperty(jsonKey, out var prop),
                    Is.True,
                    $"Serialized JSON should contain property '{jsonKey}'");
            Assert.That(prop.ToString(), Is.EqualTo(expectedValue));
        }
    }

    // ── Deserialization: JSON (from frontend) → TaskModel ────────────────────
    //
    // Verifies that each JSON key the frontend sends is bound to the correct C# property.
    // Add a row here for every new property: (json key, expected C# value, m => m.Property).
    // The expected value is a plain C# value — the test serializes it to build valid JSON input.

    private static IEnumerable<TestCaseData> DeserializationCases()
    {
        var testCases = new (string Key, object Value, Func<TaskModel, object> Get)[]
        {
            ("id",          42,           m => (object)m.Id   ),
            ("title",       "Buy milk",   m => m.Title        ),
            ("description", "Pick up 2L", m => m.Description  ),
            ("status",      "done",       m => m.Status       ),
        };

        return testCases
            .Select(tc => new TestCaseData(tc.Key, tc.Value, tc.Get)
            .SetName($"Bind JSON key '{tc.Key}' to property"));
    }

    [TestCaseSource(nameof(DeserializationCases))]
    public void Deserializes_JsonKey_ToExpectedModelProperty(
        string jsonKey, object expectedValue, Func<TaskModel, object> getProperty)
    {
        var json = $"{{\"{jsonKey}\": {JsonSerializer.Serialize(expectedValue)}}}";
        var model = JsonSerializer.Deserialize<TaskModel>(json, Options)!;

        Assert.That(getProperty(model), Is.EqualTo(expectedValue));
    }
}
