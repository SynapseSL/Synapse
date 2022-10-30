using Neuron.Core.Logging;

namespace Synapse3.SynapseModule;

public static class SynapseLogger<TName>
{
    private static ILogger _logger;
    public static ILogger Logger => _logger ??= NeuronLogger.For<TName>();

    public static void Debug(object msg) => Logger.Debug(msg);

    public static void Warn(object msg) => Logger.Warn(msg);

    public static void Info(object msg) => Logger.Info(msg);

    public static void Error(object msg) => Logger.Error(msg);

    public static void Fatal(object msg) => Logger.Fatal(msg);

    public static void Verbose(object msg) => Logger.Verbose(msg);
}
