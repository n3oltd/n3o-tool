using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using N3O.Tool.Utilities;
using NSwag;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Clients;

[Command("clients", Description = "Generate OpenAPI clients")]
public partial class ClientsCommand : CommandLineCommand {
    private readonly ILogger _logger;
    private readonly IConsole _console;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Shell _shell;

    public ClientsCommand(ILogger<ClientsCommand> logger,
                          IConsole console,
                          IHttpClientFactory httpClientFactory,
                          Shell shell) {
        _logger = logger;
        _console = console;
        _httpClientFactory = httpClientFactory;
        _shell = shell;
    }

    protected override async Task<int> OnExecuteAsync(CommandLineApplication app) {
        if (Language.Equals("CSharp", StringComparison.InvariantCultureIgnoreCase)) {
            if (string.IsNullOrWhiteSpace(Namespace)) {
                throw new ValidationException($"--namespace must be specified when generating C# clients");
            }

            await GenerateCSharpClientAsync();
        } else if (Language.Equals("TypeScript", StringComparison.InvariantCultureIgnoreCase)) {
            if (string.IsNullOrWhiteSpace(PackageName)) {
                throw new ValidationException($"--package-name must be specified when generating TypeScript clients");
            }

            if (string.IsNullOrWhiteSpace(PackageDescription)) {
                throw new ValidationException($"--package-description must be specified when generating TypeScript clients");
            }

            await GenerateTypeScriptClientAsync();
        } else {
            throw new ValidationException("Invalid language specified");
        }

        return 0;
    }

    private async Task<OpenApiDocument> GetOpenApiDocumentAsync() {
        _console.WriteLine($"Downloading {Url}");

        var httpClient = _httpClientFactory.CreateClient();

        var json = await httpClient.GetStringAsync(Url);

        _logger.LogDebug("Fetched {Json}", json);

        var openApiDocument = await OpenApiDocument.FromJsonAsync(json);

        return openApiDocument;
    }
    
    [Option("--no-models", Description = "Specifies whether models should be generated or not", ShowInHelpText = true)]
    public bool NoModels { get; set; }
    
    [Option("--exclude-models", Description = "Specify which models should be excluded (split using | )", ShowInHelpText = true)]
    public string ExcludeModels { get; set; }

    [Option("--language", Description = "The language of the client, must be one of typescript|csharp", ShowInHelpText = true)]
    [Required]
    public string Language { get; set; }

    [Option("--name", Description = "The name of the client", ShowInHelpText = true)]
    [Required]
    public string Name { get; set; }

    [Option("--namespace", Description = "The namespace of the client (when generating C# clients)", ShowInHelpText = true)]
    public string Namespace { get; set; }

    [Option("--output-path", Description = "The output path for the generated client", ShowInHelpText = true)]
    [Required]
    public string OutputPath { get; set; }

    [Option("--package-description", Description = "The npm package description (when generating TypeScript clients)", ShowInHelpText = true)]
    public string PackageDescription { get; set; }

    [Option("--package-name", Description = "The npm package name (when generating TypeScript clients)", ShowInHelpText = true)]
    public string PackageName { get; set; }

    [Option("--url", Description = "The full URL of the OpenAPI JSON file", ShowInHelpText = true)]
    [Required]
    public string Url { get; set; }
}