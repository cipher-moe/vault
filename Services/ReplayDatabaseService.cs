using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using vault.Services.ReplayDatabase;

namespace vault.Services
{
    public class RefreshMostPlayedMapsService : BackgroundService
    {
        private readonly ReplayDatabaseService replayDbService;
        private readonly BeatmapDataService beatmapDataService;
        private readonly ILogger logger;        

        public RefreshMostPlayedMapsService(
            ReplayDatabaseService replayDatabaseService,
            ILogger<ReplayDatabaseService> logger,
            BeatmapDataService beatmapDataService)
        {
            this.logger = logger;
            replayDbService = replayDatabaseService;
            this.beatmapDataService = beatmapDataService;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var aggregate = replayDbService.Collection.Aggregate()
                    .Group(replay => replay.BeatmapHash, group => new { group.Key, Count = group.Count() })
                    .SortByDescending(record => record.Count)
                    .Limit(50);
                
                replayDbService.MostPlayedMaps = aggregate.ToList(stoppingToken).Select(pair => (pair.Key, pair.Count)).ToList();
                replayDbService.LastUpdated = DateTime.Now;
                ;

                foreach (var (map, _) in replayDbService.MostPlayedMaps)
                {
                    // load this once to cache
                    await beatmapDataService.GetByHash(map);
                }
                logger.Log(LogLevel.Debug, "Loaded {0} most played maps.", replayDbService.MostPlayedMaps);
                
                await Task.Delay(30 * 1000, stoppingToken);
            }
        }
    }
    
    public class ReplayDatabaseService
    {
        public readonly IMongoCollection<Replay> Collection;
        public List<(string, int)> MostPlayedMaps = new ();
        public DateTime LastUpdated = DateTime.Now;

        public ReplayDatabaseService(MongoClient client)
        {
            
            Collection = client.GetDatabase("osu").GetCollection<Replay>("replays");
        }
    }
}