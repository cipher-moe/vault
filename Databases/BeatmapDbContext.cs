using System;
using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using Microsoft.EntityFrameworkCore;
using OsuSharp;

namespace vault.Databases
{
    public class BeatmapDbContext : DbContext
    {
        private readonly OsuClient osuClient;
        private readonly HttpClient httpClient;
        private readonly FastConcurrentLru<string, Beatmap> cache = new(500);
        private readonly FastConcurrentLru<string, string> idToHashCache = new(500);

        public BeatmapDbContext(DbContextOptions<BeatmapDbContext> options, OsuClient osuClient, HttpClient httpClient) : base(options)
        {
            this.osuClient = osuClient;
            this.httpClient = httpClient;
        }
        public DbSet<Entities.Beatmap> Beatmaps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Beatmap>().ToTable("beatmaps").HasKey(r => r.BeatmapHash);
            base.OnModelCreating(modelBuilder);
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
            var res = await osuClient.GetBeatmapByHashAsync(hash);
            cache.AddOrUpdate(hash, res);
            return res;
        } 
    }
}