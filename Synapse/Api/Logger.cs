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
            SaveMesaage(message, MessageType.Info, name);
        }

        public void Info(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Cyan);
            SaveMesaage(message, MessageType.Info, name);
        }

        public void Warn(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Green);
            SaveMesaage(message, MessageType.Warn, name);
        }

        public void Warn(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Green);
            SaveMesaage(message, MessageType.Warn, name);
        }

        public void Error(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Red);
            SaveMesaage(message, MessageType.Error, name);
        }

        public void Error(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"{name}: {message}", ConsoleColor.Red);
            SaveMesaage(message, MessageType.Error, name);
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

        internal void SaveMesaage(object message, MessageType type, string Assembly)
        {
            if (SynapseController.Server.Configs.synapseConfiguration?.SaveLog ?? false)
            {
                var save = String.Format("{0} | {1,-20}.dll | {2,-5} | {3}", DateTime.Now, Assembly, type, message);
                File.AppendAllText(Server.Get.Files.LogFile, save);
            }
        }

        public void SaveMesaage(object message, MessageType type)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            var save = String.Format("{0} | {1,-20}.dll | {2,-5} | {3}", DateTime.Now, name, type, message);
            File.AppendAllText(Server.Get.Files.LogFile, save);
        }
    }
}
