using Synapse.Api.Components;

namespace Synapse.Api.Events.SynapseEventArguments
{
    public class PlayerJoinEventArgs: EventHandler.SynapseEventArgs
    {
        public Player Player { internal set; get; }
        
        public string Nickname { set; get; }

        public void LogInConsole() => SynapseController.Server.Logger.Info(
            $"PlayerJoinEventArgs Current Args:\nValues: Player: {Player} | Nickname: {Nickname}");
    }

    public class PlayerLeaveEventArgs : EventHandler.SynapseEventArgs
    {
        public Player Player { get; internal set; }

        public void LogInConsole() =>
            SynapseController.Server.Logger.Info($"PlayerLeaveEventArgs Current Args:\nValues: Player {Player}");
    }
}