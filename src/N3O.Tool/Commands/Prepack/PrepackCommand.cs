using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using N3O.Tool.Commands.Clients;
using N3O.Tool.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace N3O.Tool.Commands; 

[Command("prepack", Description = "Prepares .csproj files for packing by adding any transitive NuGet references")]
public class PrepackCommand : CommandLineCommand {
    [Option("-p|--path", Description = "The root folder for the project, if not specified defaults to current folder", ShowInHelpText = true)]
    public string ProjectPath { get; set; }
    
    private readonly ILogger _logger;

    public PrepackCommand(ILogger<ClientsCommand> logger) {
        _logger = logger;
    }

    protected override Task<int> OnExecuteAsync(CommandLineApplication app) {
        var path = Path.GetFullPath(ProjectPath ?? ".");
        var projectFiles = GetProjectFiles(path);

        foreach (var file in projectFiles) {
            _logger.LogInformation($"Processing {file.FullName}");
            
            Console.WriteLine($"Pre-packing - {file.FullName}");
            DotnetPackerStandalone.UpdateProject(file.FullName);
        }

        return Task.FromResult(0);
    }

    private IEnumerable<FileInfo> GetProjectFiles(string path) {
        var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);

        return csprojFiles.Select(x => new FileInfo(x));
    }
}