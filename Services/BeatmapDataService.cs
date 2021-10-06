using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using MongoDB.Driver;
using OsuSharp;

namespace vault.Services
{
    public class BeatmapDataService
    {
        private readonly FastConcurrentLru<string, Beatmap> cache = new(500);
        private readonly OsuClient client;
        private readonly IMongoCollection<BeatmapData.Beatmap> collection;

        public BeatmapDataService(OsuClient client, MongoClient mongoClient)
        {
            this.client = client;
            collection = mongoClient.GetDatabase("osu").GetCollection<BeatmapData.Beatmap>("beatmap");
        }

        public async Task<long> CountBeatmap()
        {
            return await collection.EstimatedDocumentCountAsync();
        }
        
        public async Task<Beatmap?> GetByHash(string hash)
        {
            if (cache.TryGet(hash, out var @return)) return @return;
            var res = await client.GetBeatmapByHashAsync(hash);
            cache.AddOrUpdate(hash, res);
            return res;
        }
    }
}