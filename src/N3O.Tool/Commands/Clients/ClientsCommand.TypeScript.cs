using Microsoft.Extensions.Logging;
using N3O.Tool.Utilities;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.TypeScript;
using System.IO;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Clients;

public partial class ClientsCommand {
    private async Task GenerateTypeScriptClientAsync(bool connectApi) {
        var openApiDocument = await GetOpenApiDocumentAsync("2");

        var srcFolder = System.IO.Path.Combine(OutputPath, "src");
        Directory.CreateDirectory(srcFolder);

        var settings = new TypeScriptClientGeneratorSettings();

        settings.ImportRequiredTypes = true;
        
        settings.TypeScriptGeneratorSettings.ExcludedTypeNames = ExcludeModels?.Split('|') ?? [];
        settings.TypeScriptGeneratorSettings.ExportTypes = true;
        settings.TypeScriptGeneratorSettings.TypeStyle = TypeScriptTypeStyle.Interface;
        
        if (connectApi) {
            settings.ClientBaseClass = "ConnectApiBase";
            settings.ConfigurationClass = "IApiHeaders";
            settings.UseTransformOptionsMethod = true;
            
            settings.TypeScriptGeneratorSettings.ExtensionCode = EmbeddedResource.Text("ConnectApiBase.ts");
        } else {
            settings.ConfigurationClass = "IApiConfiguration";
            settings.UseTransformOptionsMethod = false;
        }

        var generator = new TypeScriptClientGenerator(openApiDocument, settings);

        var tsMain = generator.GenerateFile(ClientGeneratorOutputType.Full);
        
        await File.WriteAllTextAsync(System.IO.Path.Combine(srcFolder, "index.ts"), tsMain);

        GenerateNpmPackage();
    }

    public void GenerateNpmPackage() {
        GeneratePackageJson();
        GenerateTsConfig();
        File.WriteAllText(System.IO.Path.Combine(OutputPath, "README.md"), PackageDescription);

        _shell.Run(@"C:\Program Files\nodejs\npm.cmd", "install", workingDirectory: OutputPath).WaitForExit();
        _shell.Run(@"C:\Program Files\nodejs\npm.cmd", "run build", workingDirectory: OutputPath).WaitForExit();

        Directory.Delete(System.IO.Path.Combine(OutputPath, "node_modules"), true);
    }

    private void GeneratePackageJson() {
        var packageJson = Json.Deserialize<PackageJson>(EmbeddedResource.Text("package.json"));

        packageJson.Name = PackageName;
        packageJson.Description = PackageDescription;

        var outputFile = System.IO.Path.Combine(OutputPath, "package.json");
        var outputContent = Json.Serialize(packageJson);

        _logger.LogDebug($"Wrote the following to {outputFile}");
        _logger.LogDebug(outputContent);

        File.WriteAllText(outputFile, outputContent);
    }

    private void GenerateTsConfig() {
        var outputFile = System.IO.Path.Combine(OutputPath, "tsconfig.json");
        var outputContent = EmbeddedResource.Text("tsconfig.json");

        _logger.LogDebug($"Wrote the following to {outputFile}");
        _logger.LogDebug(outputContent);

        File.WriteAllText(outputFile, outputContent);
    }
}