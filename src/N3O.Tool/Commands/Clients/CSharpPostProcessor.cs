using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace N3O.Tool.Commands.Clients;

public class CSharpPostProcessor {
    private static readonly Regex NumberedEnumMember = new(@"(?<name>\w+) = (?<index>\d+),");

    public string ReplaceNumberEnumsWithConstants(string code,
                                                  string json,
                                                  OpenApiDocument document,
                                                  IEnumerable<string> excludedTypeNames) {
        using (var openApiJson = JsonDocument.Parse(json)) {
            foreach (var (name, schema) in document.Components.Schemas) {
                if (schema.Type.HasFlag(JsonObjectType.Number) &&
                    schema.Enumeration.Any() &&
                    !excludedTypeNames.Contains(name)) {
                    code = ReplaceNumberEnum(code, name, GetValues(name, openApiJson.RootElement));
                }
            }
        }

        return code;
    }

    private decimal GetValue(string name, JsonElement value) {
        var text = value.GetRawText();

        if (value.ValueKind == JsonValueKind.Number &&
            decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) &&
            number.ToString(CultureInfo.InvariantCulture) == text) {
            return number;
        } else {
            throw new Exception($"The number enum {name} has the value {text}, which a decimal cannot hold exactly");
        }
    }

    private IReadOnlyList<decimal> GetValues(string name, JsonElement root) {
        if (root.TryGetProperty("components", out var components) &&
            components.TryGetProperty("schemas", out var schemas) &&
            schemas.TryGetProperty(name, out var schema) &&
            schema.TryGetProperty("enum", out var values) &&
            values.ValueKind == JsonValueKind.Array) {
            return values.EnumerateArray().Select(x => GetValue(name, x)).ToList();
        } else {
            throw new Exception($"The number enum {name} has no enum under components.schemas in the document");
        }
    }

    private string ReplaceNumberEnum(string code, string name, IReadOnlyList<decimal> values) {
        var declarations = new Regex($@"^(?<indent> *)public enum {Regex.Escape(name)}\s*\{{(?<members>[^}}]*)\}}",
                                     RegexOptions.Multiline).Matches(code);

        if (declarations.Count != 1) {
            throw new Exception($"The C# client declares the number enum {name} {declarations.Count} times, " +
                                "so it cannot be replaced with constants");
        }

        var declaration = declarations[0];
        var indent = declaration.Groups["indent"].Value;
        var members = NumberedEnumMember.Matches(declaration.Groups["members"].Value);

        if (members.Count != values.Count) {
            throw new Exception($"The C# enum {name} has {members.Count} members but its schema has " +
                                $"{values.Count} values");
        }

        var constants = new StringBuilder();
        constants.Append($"{indent}public static class {name}\n{indent}{{\n");

        for (var i = 0; i < members.Count; i++) {
            var member = members[i].Groups["name"].Value;
            var index = members[i].Groups["index"].Value;

            if (index != i.ToString(CultureInfo.InvariantCulture)) {
                throw new Exception($"The C# enum {name} gives {member} the value {index} at position {i}, " +
                                    "so its members cannot be matched to the schema's values");
            }

            var value = values[i].ToString(CultureInfo.InvariantCulture);

            constants.Append($"{indent}    public const decimal {member} = {value}m;\n");
        }

        constants.Append($"{indent}}}");

        return code.Remove(declaration.Index, declaration.Length).Insert(declaration.Index, constants.ToString());
    }
}
