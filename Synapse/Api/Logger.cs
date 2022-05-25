using Synapse.Api.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Synapse.Api
{
    public class Logger
    {
        public static Logger Get
            => SynapseController.Server.Logger;

        private readonly List<string> fileLogBuffer;
        private readonly ushort bufferLengthThreshold;

        internal Logger()
        {
            fileLogBuffer = new List<string>();
            bufferLengthThreshold = 3;
        }

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
            var save = $"{DateTime.Now} | {name}.dll | {type} | {message}";

            if (Server.Get.Configs?.SynapseConfiguration?.LogMessages ?? false)
            {
                fileLogBuffer.Add(save);
                if (fileLogBuffer.Count >= bufferLengthThreshold)
                {
                    FlushLogs();
                }
            }
        }
        internal void FlushLogs()
        {
            try
            {
                if (Server.Get.Configs?.SynapseConfiguration?.LogMessages ?? false)
                    File.AppendAllLines(Server.Get.Files.LogFile, fileLogBuffer);
            }
            catch (Exception ex)
            {
                Send($"[ERR] Synapse-Logger: Saving the last log into a file failed:\n{ex}", ConsoleColor.Red);
            }

            fileLogBuffer.Clear();
        }
        internal void Refresh()
        {
            if (Server.Get.Configs.SynapseConfiguration.LogMessages)
            {
                Server.Get.Files.InitLogDirectories();
                File.AppendAllLines(Server.Get.Files.LogFile, fileLogBuffer);
            }

            fileLogBuffer.Clear();
        }
    }
}
