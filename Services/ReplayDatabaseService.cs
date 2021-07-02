using System;
using MongoDB.Driver;
using vault.Services.ReplayDatabase;

namespace vault.Services
{
    public class ReplayDatabaseService
    {
        public readonly MongoClient Client;
        public readonly IMongoDatabase Database;
        public readonly IMongoCollection<Replay> Collection;
        
        public ReplayDatabaseService()
        {
            var env = Environment.GetEnvironmentVariables();
            var uri = env["MONGODB_URI"].ToString();
            var dbName = env["MONGODB_REPLAY_DB"].ToString();
            var collectionName = env["MONGODB_REPLAY_COLLECTION"].ToString();

            Client = new MongoClient(uri);
            Database = Client.GetDatabase(dbName);
            Collection = Database.GetCollection<Replay>(collectionName);
        }
    }
}