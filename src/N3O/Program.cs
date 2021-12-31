using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using N3O.Commands.Clients;
using N3O.Utilities;
using System;

namespace N3O {
    public class Program {
        public static int Main(string[] args) {
            try {
                var services = new ServiceCollection()
                               .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                               .AddSingleton<Shell>()
                               .AddHttpClient()
                               .AddLogging(opt => {
                                   opt.AddConsole();
                                   opt.AddDebug();
                               })
                               .BuildServiceProvider();

                var app = new CommandLineApplication<Application>();

                app.Conventions
                   .UseDefaultConventions()
                   .UseConstructorInjection(services);

                return app.Execute(args);
            } catch (Exception e) {
                Console.WriteLine($"Failed with error: {e.Message}");
                Console.WriteLine(e.StackTrace);

                return -1;
            } finally {
                Shell.KillRunningProcesses();
            }
        }
    }
}