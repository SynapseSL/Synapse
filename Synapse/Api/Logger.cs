using System;
using System.Reflection;

namespace Synapse.Api
{
    public class Logger
    {
        public static Logger Get => SynapseController.Server.Logger;
        
        internal Logger() { }

        public void Info(string message) => Info((object)message);

        public void Info(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Cyan);
        }

        public void Warn(string message) => Warn((object)message);

        public void Warn(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Green);
        }

        public void Error(string message) => Error((object)message);

        public void Error(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Red);
        }

        public void Send(string message, ConsoleColor color) => ServerConsole.AddLog(message, color);
    }
}
