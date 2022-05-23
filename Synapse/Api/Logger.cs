using System;
using System.IO;
using System.Reflection;
using Synapse.Api.Enum;
using System.Collections.Generic;

namespace Synapse.Api
{
    public class Logger
    {
        public static Logger Get => SynapseController.Server.Logger;

        internal Logger() { }

        public void Info(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"[INF] {name}: {message}", ConsoleColor.Cyan);
            SaveMessage(message, MessageType.Info, name);
        }

        public void Info(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"[INF] {name}: {message}", ConsoleColor.Cyan);
            SaveMessage(message, MessageType.Info, name);
        }

        public void Warn(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"[WRN] {name}: {message}", ConsoleColor.Green);
            SaveMessage(message, MessageType.Warn, name);
        }

        public void Warn(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"[WRN] {name}: {message}", ConsoleColor.Green);
            SaveMessage(message, MessageType.Warn, name);
        }

        public void Error(string message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"[ERR] {name}: {message}", ConsoleColor.Red);
            SaveMessage(message, MessageType.Error, name);
        }

        public void Error(object message)
        {
            var name = Assembly.GetCallingAssembly().GetName().Name;
            Send($"[ERR] {name}: {message}", ConsoleColor.Red);
            SaveMessage(message, MessageType.Error, name);
        }

        internal void Debug(object message)
        {
            if (SynapseVersion.Debug)
            {
                Send($"Synapse-Debug: {message}", ConsoleColor.DarkYellow);
                SaveMessage(message, MessageType.Debug);
            }
        }

        public void Send(object message, ConsoleColor color)
            => ServerConsole.AddLog(message.ToString(), color);

        public void Send(string message, ConsoleColor color)
            => ServerConsole.AddLog(message, color);

        public void SaveMessage(object message, MessageType type)
            => SaveMessage(message, type, Assembly.GetCallingAssembly().GetName().Name);

        public void SaveMessage(object message, MessageType type, string name)
        {
            try
            {
                var save = $"{DateTime.Now} | {name}.dll | {type} | {message}";

                if (logEnabled)
                    File.AppendAllText(Server.Get.Files.LogFile, save + "\n");
                else if (Server.Get.Configs?.SynapseConfiguration?.LogMessages != false)
                    messages.Add(save);
            }
            catch (Exception ex)
            {
                Send($"[ERR] Synapse-Logger: Saving the last log into a file failed:\n{ex}", ConsoleColor.Red);
            }
        }


        private readonly List<string> messages = new();
        private bool logEnabled = false;

        internal void Refresh()
        {
            if (Server.Get.Configs.SynapseConfiguration.LogMessages)
            {
                logEnabled = true;
                Server.Get.Files.InitLogDirectories();
                File.AppendAllLines(Server.Get.Files.LogFile, messages);
            }
            else
                logEnabled = false;

            messages.Clear();
        }
    }
}