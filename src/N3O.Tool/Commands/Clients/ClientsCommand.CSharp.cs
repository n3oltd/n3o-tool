﻿using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;
using System.IO;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Clients; 

public partial class ClientsCommand {
    private async Task GenerateCSharpClientAsync() {
        var openApiDocument = await GetOpenApiDocumentAsync("1");

        var settings = new CSharpClientGeneratorSettings();

        settings.ClassName = Name;
        settings.ExposeJsonSerializerSettings = true;
        settings.GenerateClientInterfaces = true;
        settings.GenerateDtoTypes = !NoModels;
        settings.CSharpGeneratorSettings.ExcludedTypeNames = ExcludeModels?.Split('|') ?? [];
        settings.CSharpGeneratorSettings.GenerateOptionalPropertiesAsNullable = true;

        settings.CSharpGeneratorSettings.Namespace = Namespace;
        settings.CSharpGeneratorSettings.JsonLibrary = CSharpJsonLibrary.NewtonsoftJson;
        settings.CSharpGeneratorSettings.TypeNameGenerator = new CSharpTypeNameGenerator();

        var generator = new CSharpClientGenerator(openApiDocument, settings);
        
        var csClient = generator.GenerateFile(ClientGeneratorOutputType.Full);

        await File.WriteAllTextAsync(System.IO.Path.Combine(OutputPath, $"{Name}.cs"), csClient);
    }
}