using System;
using System.IO;
using System.Reflection;
using Synapse.Api.Enum;

namespace Synapse.Api
{
    public class Logger
    {
        public static Logger Get => SynapseController.Server.Logger;
        
        internal Logger() { }

        public void Info(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Cyan);
            SaveMesaage(message, MessageType.Info);
        }

        public void Info(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Cyan);
            SaveMesaage(message, MessageType.Info);
        }

        public void Warn(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Green);
            SaveMesaage(message, MessageType.Warn);
        }

        public void Warn(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Green);
            SaveMesaage(message, MessageType.Warn);
        }

        public void Error(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Red);
            SaveMesaage(message, MessageType.Error);
        }

        public void Error(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Red);
            SaveMesaage(message, MessageType.Error);
        }

        internal void Debug(object message)
        {
            if (SynapseVersion.Debug)
            {
                Send($"Synapse-Debug: {message}", ConsoleColor.DarkYellow);
                SaveMesaage(message, MessageType.Debug);
            }
        }

        public void Send(string message, ConsoleColor color) => ServerConsole.AddLog(message, color);

        public void SaveMesaage(object message, MessageType type)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            var save = $"{DateTime.Now} | {name}.dll | {type} | {message} \n";
            File.AppendAllText(Server.Get.Files.LogFile, save);
        }
    }
}
