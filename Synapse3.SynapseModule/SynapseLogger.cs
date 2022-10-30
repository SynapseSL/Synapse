using System;
using Neuron.Core.Logging;

namespace Synapse3.SynapseModule
{
    public class SynapseLogger
    {
        //A Custom Synapse3 Logger, Uses Neuronlogger Utilites by Default.

        public static void Debug(string msg)
        {
            NeuronLogger.For<Synapse>().Debug(msg);
        }

        public static void Warn(string msg)
        {
            NeuronLogger.For<Synapse>().Warn(msg);
        }

        public static void Log(string msg, object[] args)
        {
            NeuronLogger.For<Synapse>().Log(LogLevel.Information, msg, args, true);
        }

        public static void Info(string msg)
        {
            NeuronLogger.For<Synapse>().Info(msg);
        }

        public static void Error(string msg)
        {
            NeuronLogger.For<Synapse>().Error(msg);
        }

        public static void Fatal(string msg)
        {
            NeuronLogger.For<Synapse>().Fatal(msg);
        }

    }
}
