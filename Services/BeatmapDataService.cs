using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using OsuSharp;

namespace vault.Services
{
    public class BeatmapDataService
    {
        private readonly FastConcurrentLru<string, Beatmap> cache = new(500);
        private readonly OsuClient client;
        
        public BeatmapDataService(OsuClient client)
        {
            this.client = client;
        }

        public async Task<Beatmap> GetByHash(string hash)
        {
            if (cache.TryGet(hash, out var @return)) return @return;
            var res = await client.GetBeatmapByHashAsync(hash);
            cache.AddOrUpdate(hash, res);
            return res;
        }
    }
}