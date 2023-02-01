using System.Collections.Generic;
using System.IO;
using MEC;
using Neuron.Core;
using Neuron.Core.Platform;
using Neuron.Core.Scheduling;

namespace Synapse3.Platform;

public class SynapseStandalonePlatform : IPlatform
{
    public static void Main()
    {
        var entrypoint = new SynapseStandalonePlatform();
        entrypoint.Boostrap();
    }
    
    public PlatformConfiguration Configuration { get; set; } = new();
    public ActionCoroutineReactor CoroutineReactor = new();
    public NeuronBase NeuronBase { get; set; }

    private CoroutineHandle _mainCoroutineHandle;
    
    public void Load()
    {
        Configuration.ConsoleWidth = 85;
        Configuration.BaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Synapse");
        Configuration.FileIo = true;
        Configuration.CoroutineReactor = CoroutineReactor;
        Configuration.OverrideConsoleEncoding = false;
        Configuration.EnableConsoleLogging = false;
        Configuration.LogEventSink = new LabLogRenderer();
    }

    public void Enable()
    {
        _mainCoroutineHandle = Timing.RunCoroutine(TickCoroutines());
    }

    private IEnumerator<float> TickCoroutines()
    {
        var ticker = CoroutineReactor.GetTickAction();
        while (true)
        {
            ticker.Invoke();
            yield return 0;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public void Continue()
    {
        
    }

    public void Disable()
    {
        
    }
}