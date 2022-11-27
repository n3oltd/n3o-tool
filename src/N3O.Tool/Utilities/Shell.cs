using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace N3O.Tool.Utilities;

public class Shell {
    private static readonly ConcurrentBag<Process> Processes = new();
    private readonly ILogger _logger;

    public Shell(ILogger<Shell> logger) {
        _logger = logger;
    }

    public Process Run(string command,
                       string args,
                       Action<string> outputReceiver = null,
                       IDictionary<string, string> environment = null,
                       string workingDirectory = null) {
        var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = string.Join(" ", args);
        process.StartInfo.RedirectStandardOutput = true;

        if (outputReceiver == null) {
            outputReceiver = x => _logger.LogDebug(x);
        }

        process.OutputDataReceived += (_, eventArgs) => outputReceiver(eventArgs.Data);

        if (environment != null) {
            foreach (var (key, value) in environment) {
                process.StartInfo.Environment[key] = value;
            }
        }

        if (workingDirectory != null) {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        _logger.LogDebug("Running {FileName} with {Arguments}",
                         process.StartInfo.FileName,
                         process.StartInfo.Arguments);

        if (environment != null && environment.Any()) {
            _logger.LogDebug("Environment: {Environment}", environment);
        }

        process.Start();
        process.BeginOutputReadLine();

        Processes.Add(process);

        return process;
    }

    public static void KillRunningProcesses() {
        foreach (var process in Processes) {
            try {
                if (IsRunning(process)) {
                    process.Kill(true);
                }
            } catch (Exception) { }
        }
    }

    private static bool IsRunning(Process process) {
        try {
            Process.GetProcessById(process.Id);

            return true;
        } catch (Exception) {
            return false;
        }
    }
}