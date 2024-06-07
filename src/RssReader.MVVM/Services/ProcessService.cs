using System.Diagnostics;
using RssReader.MVVM.Services.Interfaces;

namespace RssReader.MVVM.Services;

public class ProcessService : IProcessService
{
    public void Run(string command, string arguments)
    {
        var process = GetProcess(command, arguments);
        process.Start();
    }

    private static Process GetProcess(string command, string arguments, bool redirectOutput = false)
    {
        var processStartInfo = new ProcessStartInfo(command)
        {
            RedirectStandardOutput = redirectOutput,
            UseShellExecute = false,
            CreateNoWindow = false,
            Arguments = arguments
        };

        return new Process { StartInfo = processStartInfo };
    }
}
