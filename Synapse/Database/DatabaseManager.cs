using System.Collections.Generic;
using System.Data;
using System.IO;
using LiteDB;

namespace Synapse.Database
{
    public class DatabaseManager
    {

        public static LiteDatabase LiteDatabase => !SynapseController.Server.Configs.SynapseConfiguration.DatabaseEnabled ? null :
            new LiteDatabase(Path.Combine(SynapseController.Server.Files.DatabaseDirectory,
                SynapseController.Server.Configs.SynapseConfiguration.DatabaseShared ? "database.db" : $"server-{ServerStatic.ServerPort}.db"));

        public static void CheckEnabledOrThrow()
        {
            if (!SynapseController.Server.Configs.SynapseConfiguration.DatabaseEnabled) throw new DataException("The Database has been disabled in the config. " +
                "Please check SynapseController.EnableDatabase before accessing connected APIs");
        }

        public static PlayerRepository PlayerRepository = new();

    }

    public class PlayerRepository : Repository<PlayerDbo>
    {
        public PlayerDbo FindByGameId(string id)
        {
            return Get(LiteDB.Query.EQ("GameIdentifier", id));
        }

        public bool ExistGameId(string id)
        {
            return Exists(LiteDB.Query.EQ("GameIdentifier", id));
        }
    }

    public class PlayerDbo : IDatabaseEntity
    {
        public int Id { get; set; }

        public string GameIdentifier { get; set; }

        public string Name { get; set; }

        public bool DoNotTrack { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public int GetId()
        {
            return Id;
        }
    }

}