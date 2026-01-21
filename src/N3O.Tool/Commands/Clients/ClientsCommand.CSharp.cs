using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Clients; 

public partial class ClientsCommand {
    private async Task GenerateCSharpClientAsync() {
        var openApiDocument = await GetOpenApiDocumentAsync("1");

        var settings = new CSharpClientGeneratorSettings();

        var typesToRemove = new List<string>();
        
        if (IncludeModels?.Split('|').Any() == true) {
            var schemas = openApiDocument.Components.Schemas;
            
            typesToRemove = schemas.Keys.Where(k => !IncludeModels.Split('|').Contains(k)).ToList();
        } else if (ExcludeModels?.Split('|').Any() == true) {
            typesToRemove.AddRange(ExcludeModels.Split('|'));
        }
        
        settings.ClassName = Name;
        settings.ExposeJsonSerializerSettings = true;
        settings.GenerateClientInterfaces = true;
        settings.GenerateDtoTypes = !NoModels;
        settings.CSharpGeneratorSettings.ExcludedTypeNames = typesToRemove.ToArray();
        settings.CSharpGeneratorSettings.GenerateOptionalPropertiesAsNullable = true;

        settings.CSharpGeneratorSettings.Namespace = Namespace;
        settings.CSharpGeneratorSettings.JsonLibrary = CSharpJsonLibrary.NewtonsoftJson;
        settings.CSharpGeneratorSettings.TypeNameGenerator = new CSharpTypeNameGenerator();

        var generator = new CSharpClientGenerator(openApiDocument, settings);
        
        var csClient = generator.GenerateFile(ClientGeneratorOutputType.Full);

        await File.WriteAllTextAsync(System.IO.Path.Combine(OutputPath, $"{Name}.cs"), csClient);
    }
    
    private static HashSet<string> CollectSchemasWithDependencies(
        OpenApiDocument document,
        IEnumerable<string> rootSchemas) {

        var result = new HashSet<string>(rootSchemas);
        var queue = new Queue<string>(rootSchemas);

        while (queue.Count > 0) {
            var current = queue.Dequeue();

            if (!document.Components.Schemas.TryGetValue(current, out var schema)) {
                continue;
            }

            foreach (var referenced in GetReferencedSchemas(schema)) {
                if (result.Add(referenced)) {
                    queue.Enqueue(referenced);
                }
            }
        }

        return result;
    }

    private static IEnumerable<string> GetReferencedSchemas(JsonSchema schema) {
        var referencedSchemas = new List<JsonSchema>();

        if (schema.Reference != null)
            referencedSchemas.Add(schema.Reference);

        referencedSchemas.AddRange(schema.AllOf);
        referencedSchemas.AddRange(schema.OneOf);
        referencedSchemas.AddRange(schema.AnyOf);

        foreach (var property in schema.Properties.Values) {
            if (property.Reference != null)
                referencedSchemas.Add(property.Reference);

            referencedSchemas.AddRange(property.AllOf);
            referencedSchemas.AddRange(property.OneOf);
            referencedSchemas.AddRange(property.AnyOf);
        }

        return referencedSchemas
              .Where(s => s?.Title != null)
              .Select(s => s.Title!)
              .Distinct();
    }
}