using McMaster.Extensions.CommandLineUtils;
using N3O.Utilities;
using Spectre.Console;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace N3O {
    [Command]
    [VersionOptionFromMember("--version", MemberName = nameof(DisplayVersion))]
    public class Application : CommandLineCommand {
        protected override Task<int> OnExecuteAsync(CommandLineApplication app) {
            Figlet("N3O");
            
            app.ShowHelp();

            return Task.FromResult(1);
        }

        private static void Figlet(string text) {
            var font = FigletFont.Load(EmbeddedResource.Text("speed.flf"));
            var figlet = new FigletText(font, text);
            
            AnsiConsole.Write(figlet.Centered().Color(Color.White));
        }
        
        public static Version AppVersion => Assembly.GetEntryAssembly().GetName().Version;
        public static string DisplayVersion => AppVersion.ToString();
    }
}