using System;
using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using MongoDB.Driver;
using OsuSharp;

namespace vault.Services
{
    public class BeatmapDataService
    {
        private readonly FastConcurrentLru<string, Beatmap> cache = new(500);
        private readonly FastConcurrentLru<string, string> idToHashCache = new(500);
        private readonly OsuClient client;
        private readonly IMongoCollection<BeatmapData.Beatmap> collection;
        private readonly HttpClient httpClient;

        public BeatmapDataService(OsuClient client, MongoClient mongoClient, HttpClient httpClient)
        {
            this.client = client;
            this.httpClient = httpClient;
            collection = mongoClient.GetDatabase("osu").GetCollection<BeatmapData.Beatmap>("beatmap");
        }

        public async Task<long> CountBeatmap()
        {
            return await collection.EstimatedDocumentCountAsync();
        }
        
        public async Task<string?> GetBeatmapHash(string id)
        {
            if (idToHashCache.TryGet(id, out var hash)) return hash;
            try
            {
                var stream = await httpClient.GetStreamAsync("https://osu.ppy.sh/osu/" + id);
                using var hasher = System.Security.Cryptography.MD5.Create();
                var md5 = await hasher.ComputeHashAsync(stream);
                hash = BitConverter.ToString(md5).Replace("-", "").ToLower();
            }
            catch
            {
                return null;
            }
            idToHashCache.AddOrUpdate(id, hash);
            return hash;
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