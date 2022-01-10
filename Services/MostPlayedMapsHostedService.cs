using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using vault.Databases;

namespace vault.Services
{
    public class MostPlayedMapsHostedService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly MostPlayedMapsService dataInstance;

        public MostPlayedMapsHostedService(ILogger<MostPlayedMapsHostedService> logger, IServiceProvider serviceProvider, MostPlayedMapsService dataInstance)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.dataInstance = dataInstance;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceProvider.CreateScope();
                var replayDbContext = scope.ServiceProvider.GetRequiredService<ReplayDbContext>();
                var beatmapDbContext = scope.ServiceProvider.GetRequiredService<BeatmapDbContext>();

                
                var aggregate = await replayDbContext.AggregateDbSet
                    .FromSqlRaw(
                        "SELECT `beatmap_hash`, COUNT(`sha256`) as `count` FROM `replays` GROUP BY `beatmap_hash` ORDER BY `count` DESC LIMIT 100")
                    .ToListAsync(stoppingToken);
                
                dataInstance.MostPlayedMaps = aggregate
                    .Select(pair => (pair.BeatmapHash, pair.Count)).ToList();
                dataInstance.LastUpdated = DateTime.Now;

                foreach (var (map, _) in dataInstance.MostPlayedMaps)
                {
                    // load this once to cache
                    await beatmapDbContext.GetByHash(map);
                }
                logger.Log(LogLevel.Debug, "Loaded {MostPlayerMaps} most played maps", dataInstance.MostPlayedMaps);
                
                await Task.Delay(15 * 1000, stoppingToken);
            }
        }
    }
}