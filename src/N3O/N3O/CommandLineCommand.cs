using McMaster.Extensions.CommandLineUtils;
using System.Threading.Tasks;

namespace N3O {
    public abstract class CommandLineCommand {
        [Option("-verbose", Description = "Show verbose output, useful for debugging")]
        public bool Verbose { get; set; }

        protected abstract Task<int> OnExecuteAsync(CommandLineApplication app);
    }
}