using System;
using System.Text;
using Neuron.Core.Logging;
using Neuron.Core.Logging.Processing;

namespace Synapse3.Platform;

public class LabLogRenderer : ILogRender
{
    public void Render(LogOutput output)
    {
        var buffer = new StringBuilder();
        var color = output.Level switch
        {
            LogLevel.Verbose => ConsoleColor.DarkGray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Information => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed,
            _ => throw new ArgumentOutOfRangeException()
        };

        foreach (var token in output.Tokens)
        {
            buffer.Append(token.Message);
        }
        ServerConsole.AddLog(buffer.ToString(), color);
    }
}