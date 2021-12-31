using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using N3O.Tool.Utilities;
using Spectre.Console;
using System;
using System.ComponentModel.DataAnnotations;

namespace N3O.Tool {
    public class Program {
        public static int Main(string[] args) {
            try {
                var services = new ServiceCollection()
                               .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                               .AddSingleton<Shell>()
                               .AddHttpClient()
                               .AddLogging(opt => {
                                   opt.AddDebug();
                               })
                               .BuildServiceProvider();

                var app = new CommandLineApplication<Application>();

                app.Conventions
                   .UseDefaultConventions()
                   .UseConstructorInjection(services);

                return app.Execute(args);
            } catch (Exception e) {
                AnsiConsole.Foreground = Color.Red;

                if (e is ValidationException validationException) {
                    AnsiConsole.WriteLine(validationException.Message);
                } else {
                    AnsiConsole.WriteLine($"Fatal error: {e.Message}");
                    AnsiConsole.WriteLine(e.StackTrace);
                }

                return -1;
            } finally {
                Shell.KillRunningProcesses();
            }
        }
    }
}