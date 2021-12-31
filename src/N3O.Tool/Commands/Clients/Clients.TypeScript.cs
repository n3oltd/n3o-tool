using Microsoft.Extensions.Logging;
using N3O.Tool.Utilities;
using Newtonsoft.Json;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.TypeScript;
using System.IO;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Clients {
    public partial class Clients {
        private async Task GenerateTypeScriptClientAsync() {
            var openApiDocument = await GetOpenApiDocumentAsync();
            
            var srcFolder = Path.Combine(OutputPath, "src");
            Directory.CreateDirectory(srcFolder);
            
            var settings = new TypeScriptClientGeneratorSettings();

            settings.ClassName = Name;
            settings.ConfigurationClass = "IConfig";
            settings.ImportRequiredTypes = true;
            settings.UseTransformOptionsMethod = true;
                    
            settings.TypeScriptGeneratorSettings.TypeStyle = TypeScriptTypeStyle.Interface;
            settings.TypeScriptGeneratorSettings.ExportTypes = true;

            var generator = new TypeScriptClientGenerator(openApiDocument, settings);

            var tsMain = generator.GenerateFile(ClientGeneratorOutputType.Full);
            await File.WriteAllTextAsync(Path.Combine(srcFolder, "index.ts"), tsMain);

            GenerateNpmPackage();
        }
        
        public void GenerateNpmPackage() {
            GeneratePackageJson();
            GenerateTsConfig();
            File.WriteAllText(Path.Combine(OutputPath, "README.md"), "TODO");
            
            _shell.Run("npm", "i -D shx", workingDirectory: OutputPath).WaitForExit();
            _shell.Run("npm", "run prepack", workingDirectory: OutputPath).WaitForExit();

            Directory.Delete(Path.Combine(OutputPath, "node_modules"), true);
        }

        private void GeneratePackageJson() {
            var packageJson = JsonConvert.DeserializeObject<PackageJson>(EmbeddedResource.Text("package.json"));

            packageJson.Name = $"@n3o/{PackageName}";
            packageJson.Description = "TODO";

            var outputFile = Path.Combine(OutputPath, "package.json");
            var outputContent = JsonConvert.SerializeObject(packageJson);

            _logger.LogDebug($"Wrote the following to {outputFile}");
            _logger.LogDebug(outputContent);

            File.WriteAllText(outputFile, outputContent);
        }

        private void GenerateTsConfig() {
            var outputFile = Path.Combine(OutputPath, "tsconfig.json");
            var outputContent = EmbeddedResource.Text("tsconfig.json");

            _logger?.LogDebug($"Wrote the following to {outputFile}");
            _logger?.LogDebug(outputContent);
            
            File.WriteAllText(outputFile, outputContent);
        }
    }
}