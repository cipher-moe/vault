using System;
using MongoDB.Driver;
using vault.Services.ReplayDatabase;

namespace vault.Services
{
    public class ReplayDatabaseService
    {
        
        public readonly IMongoCollection<Replay> Collection;
        
        public ReplayDatabaseService()
        {
            var env = Environment.GetEnvironmentVariables();
            var uri = env["MONGODB_URI"].ToString();

            var client = new MongoClient(uri);
            Collection = client.GetDatabase("osu").GetCollection<Replay>("replays");
        }
    }
}