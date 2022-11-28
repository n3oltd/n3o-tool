using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using N3O.Tool.Commands.Templates.Clients;
using N3O.Tool.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Templates;

[Command("preview", Description = "Generate a preview of a template")]
public class PreviewCommand : CommandLineCommand {
    private readonly ILogger _logger;
    private readonly IConsole _console;
    private readonly ClientFactory<TemplatesClient> _clientFactory;

    public PreviewCommand(ILogger<PreviewCommand> logger,
                          IConsole console,
                          ClientFactory<TemplatesClient> clientFactory) {
        _logger = logger;
        _console = console;
        _clientFactory = clientFactory;
    }

    protected override async Task<int> OnExecuteAsync(CommandLineApplication app) {
        var client = _clientFactory.Create(SubscriptionId);

        var markup = await File.ReadAllTextAsync(InputFile);
        var mergeModels = await GetMergeModelsAsync();

        _logger.LogDebug("Rendering template");

        Stream output;

        if (Format.Equals("email", StringComparison.InvariantCultureIgnoreCase)) {
            output = await RenderEmailAsync(client, markup, mergeModels);
        } else if (Format.Equals("pdf", StringComparison.InvariantCultureIgnoreCase)) {
            output = await RenderPdfAsync(client, markup, mergeModels);
        } else if (Format.Equals("sms", StringComparison.InvariantCultureIgnoreCase)) {
            output = await RenderSmsAsync(client, markup, mergeModels);
        } else {
            throw new ValidationException("Invalid format specified");
        }

        await WriteOutputFileAsync(output);

        _console.WriteLine("Success");

        return 0;
    }

    private async Task<List<MergeModelReq>> GetMergeModelsAsync() {
        if (!string.IsNullOrWhiteSpace(MergeModelsFile)) {
            _logger.LogDebug("Deserializing merge models");

            var mergeModels = new List<MergeModelReq>();

            var json = await File.ReadAllTextAsync(MergeModelsFile);
            var jToken = JsonConvert.DeserializeObject(json);

            if (jToken is JArray jArray) {
                foreach (var jObject in jArray.OfType<JObject>()) {
                    mergeModels.Add(jObject.ToObject<MergeModelReq>());
                }
            } else if (jToken is JObject jObject) {
                mergeModels.Add(jObject.ToObject<MergeModelReq>());
            } else {
                throw new ValidationException("Merge model JSON is invalid");
            }

            return mergeModels;
        } else {
            _logger.LogDebug("No merge models specified");

            return null;
        }
    }

    private async Task<Stream> RenderEmailAsync(ServiceClient<TemplatesClient> client,
                                                string markup,
                                                List<MergeModelReq> mergeModels) {
        var req = new PreviewEmailCompositionReq();
        req.CompositionId = TemplateId;
        req.Markup = markup;
        req.MergeModels = mergeModels;

        var res = await client.InvokeAsync<PreviewEmailCompositionReq, RenderEmailRes>(x => x.EmailTemplatePreviewAsync,
                                                                                       req);

        return new MemoryStream(Encoding.UTF8.GetBytes(res.Body));
    }

    private async Task<Stream> RenderPdfAsync(ServiceClient<TemplatesClient> client,
                                              string markup,
                                              List<MergeModelReq> mergeModels) {
        var req = new PreviewPdfCompositionReq();
        req.CompositionId = TemplateId;
        req.Markup = markup;
        req.MergeModels = mergeModels;

        var fileResponse = await client.InvokeAsync<PreviewPdfCompositionReq, FileResponse>(x => x.PdfTemplatePreviewAsync,
                                                                                            req);

        return fileResponse.Stream;
    }

    private async Task<Stream> RenderSmsAsync(ServiceClient<TemplatesClient> client,
                                              string markup,
                                              List<MergeModelReq> mergeModels) {
        var req = new PreviewSmsCompositionReq();
        req.CompositionId = TemplateId;
        req.Markup = markup;
        req.MergeModels = mergeModels;

        var res = await client.InvokeAsync<PreviewSmsCompositionReq, RenderSmsRes>(x => x.SmsTemplatePreviewAsync, req);

        return new MemoryStream(Encoding.UTF8.GetBytes(res.Body));
    }

    private async Task WriteOutputFileAsync(Stream output) {
        if (File.Exists(OutputFile)) {
            File.Delete(OutputFile);
        }

        using (var fileStream = File.OpenWrite(OutputFile)) {
            await output.CopyToAsync(fileStream);
        }
    }

    [Option("-f|--format", Description = "The format of the template, must be one of email|pdf|sms", ShowInHelpText = true)]
    [Required]
    public string Format { get; set; }

    [Option("-i|--input-file", Description = "The path to the input file", ShowInHelpText = true)]
    [Required]
    public string InputFile { get; set; }

    [Option("-m|--models-file", Description = "The path to a file containing any custom merge models", ShowInHelpText = true)]
    public string MergeModelsFile { get; set; }

    [Option("-o|--output-file", Description = "The path to the output file", ShowInHelpText = true)]
    [Required]
    public string OutputFile { get; set; }

    [Option("-s|--subscription-id", Description = "The subscription ID of the template", ShowInHelpText = true)]
    [Required]
    public string SubscriptionId { get; set; }

    [Option("-t|--template-id", Description = "The ID of the template", ShowInHelpText = true)]
    [Required]
    public string TemplateId { get; set; }
}