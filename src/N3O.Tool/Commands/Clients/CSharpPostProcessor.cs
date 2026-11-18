using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace N3O.Tool.Commands.Clients;

public class CSharpPostProcessor {
    private static readonly Regex EnumMember = new(@"(?<name>\w+) = (?<index>\d+),");

    public string ReplaceNumberEnumsWithConstants(string code,
                                                  string json,
                                                  OpenApiDocument document,
                                                  IEnumerable<string> excludedTypeNames) {
        var root = Parse(json);

        foreach (var (name, schema) in document.Components.Schemas) {
            if (schema.Type.HasFlag(JsonObjectType.Number) &&
                schema.Enumeration.Any() &&
                !excludedTypeNames.Contains(name)) {
                code = ReplaceNumberEnum(code, name, GetValues(root, name));
            }
        }

        return code;
    }

    private decimal GetValue(string name, JToken value) {
        if (value.Type == JTokenType.Float || value.Type == JTokenType.Integer) {
            return value.Value<decimal>();
        } else {
            throw new Exception($"The number enum {name} has the value {value.ToString(Formatting.None)}, " +
                                "which is not a number");
        }
    }

    private IReadOnlyList<decimal> GetValues(JToken root, string name) {
        if (root["components"]?["schemas"]?[name]?["enum"] is JArray values) {
            return values.Select(x => GetValue(name, x)).ToList();
        } else {
            throw new Exception($"The number enum {name} has no enum under components.schemas in the document");
        }
    }

    private JToken Parse(string json) {
        using (var reader = new JsonTextReader(new StringReader(json))) {
            reader.FloatParseHandling = FloatParseHandling.Decimal;

            return JToken.ReadFrom(reader);
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
        var members = EnumMember.Matches(declaration.Groups["members"].Value);

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
