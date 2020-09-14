using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using LiteDB;

namespace Synapse.Database
{
    public class DatabaseManager
    {

        public static LiteDatabase LiteDatabase => !SynapseController.EnableDatabase ? null : new LiteDatabase(Server.Get.Files.DatabaseFile);

        public static void CheckEnabledOrThrow()
        {
            if (!SynapseController.EnableDatabase) throw new DataException("The Database has been disabled in the config. " +
                                                                           "Please check SynapseController.EnableDatabase before accessing connected APIs");
        }
        
        public static PlayerRepository PlayerRepository = new PlayerRepository();

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
        
        public Dictionary<string,string> Data { get; set; }
        
        public int GetId()
        {
            return Id;
        }
    }

}