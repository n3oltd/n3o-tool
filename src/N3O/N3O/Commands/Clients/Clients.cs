using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using N3O.Utilities;
using NSwag;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;

namespace N3O.Commands.Clients {
    [Command("clients", Description = "Generate OpenAPI clients")]
    public partial class Clients : CommandLineCommand {
        private readonly ILogger _logger;
        private readonly IConsole _console;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Shell _shell;

        public Clients(ILogger<Clients> logger, IConsole console, IHttpClientFactory httpClientFactory, Shell shell) {
            _logger = logger;
            _console = console;
            _httpClientFactory = httpClientFactory;
            _shell = shell;
        }
        
        protected override async Task<int> OnExecuteAsync(CommandLineApplication app) {
            if (Language == "CSharp") {
                await GenerateCSharpClientAsync();
            } else if (Language == "TypeScript") {
                await GenerateTypeScriptClientAsync();
            } else {
                throw new NotImplementedException();
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
        
        [Option("--language", Description = "The language of the client, must be TypeScript or CSharp", ShowInHelpText = true)]
        [Required]
        public string Language { get; set; }
        
        [Option("--name", Description = "The name of the client", ShowInHelpText = true)]
        [Required]
        public string Name { get; set; }
        
        [Option("--namespace", Description = "The namespace of the client", ShowInHelpText = true)]
        [Required]
        public string Namespace { get; set; }

        [Option("--output-path", Description = "The output path for the generated client", ShowInHelpText = true)]
        [Required]
        public string OutputPath { get; set; }
        
        [Option("--package-name", Description = "The npm package name (when generating TypeScript clients)", ShowInHelpText = true)]
        [Required]
        public string PackageName { get; set; }

        [Option("-u|--url", Description = "The full URL of the OpenAPI JSON file", ShowInHelpText = true)]
        [Required]
        public string Url { get; set; }
    }
}