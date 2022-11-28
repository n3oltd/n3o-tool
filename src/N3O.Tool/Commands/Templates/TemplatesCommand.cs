using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace N3O.Tool.Commands.Templates;

[Command("templates", Description = "Template related commands")]
[Subcommand(typeof(PreviewCommand))]
public class TemplatesCommand : CommandLineCommand {
    protected override Task<int> OnExecuteAsync(CommandLineApplication app) {
        app.ShowHelp();

        return Task.FromResult(1);
    }
}