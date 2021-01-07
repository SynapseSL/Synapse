using System;
using System.Reflection;

namespace Synapse.Api
{
    public class Logger
    {
        public static Logger Get => SynapseController.Server.Logger;
        
        internal Logger() { }

        public void Info(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Cyan);
        }

        public void Warn(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Green);
        }

        public void Error(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Red);
        }

        public void Send(string message, ConsoleColor color) => ServerConsole.AddLog(message, color);
    }
}
