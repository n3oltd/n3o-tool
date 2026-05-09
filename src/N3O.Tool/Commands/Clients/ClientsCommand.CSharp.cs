using N3O.Tool.Extensions;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Clients; 

public partial class ClientsCommand {
    private async Task GenerateCSharpClientAsync() {
        var openApiDocument = await GetOpenApiDocumentAsync("csharp");

        var settings = new CSharpClientGeneratorSettings();

        var typesToRemove = new List<string>();
        
        if (IncludeModels.HasValue()) {
            var includeModels = IncludeModels.SplitArgs();
            
            typesToRemove.AddRange(openApiDocument.Components.Schemas.Keys.Where(k => !includeModels.Contains(k)));
        } else if (ExcludeModels.HasValue() || ExcludeModelsFrom.HasValue()) {
            var toExclude = await GetTypesToExcludeAsync();
            
            typesToRemove.AddRange(ExcludeModels.SplitArgs().Concat(toExclude).Distinct());
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
    private async Task<IReadOnlyList<string>> GetTypesToExcludeAsync() {
        var toExclude = new List<string>();
        
        foreach (var url in ExcludeModelsFrom.SplitArgs()) {
            var openApiDocument = await GetOpenApiDocumentAsync(url, "csharp");

            toExclude.AddRange(openApiDocument.Components.Schemas.Keys);
        }

        return toExclude.Distinct().ToList();
    }
}