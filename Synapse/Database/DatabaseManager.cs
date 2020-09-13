using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LiteDB;

namespace Synapse.Database
{
    public class DatabaseManager
    {

        public static LiteDatabase LiteDatabase => new LiteDatabase(Path.Combine(SynapseController.Server.Files.DatabaseDirectory, "database.db"));
        
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
    
    public class TestRepository : Repository<TestPoco>
    {
        
    }

    public class TestPoco : IDatabaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return $"TestPoc(Id={Id} Name={Name} Surname={Surname} Age={Age})";
        }

        public int GetId() => Id;
    }

}