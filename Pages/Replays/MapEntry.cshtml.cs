using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using OsuSharp;
using vault.Services;
using Replay = vault.Services.ReplayDatabase.Replay;

namespace vault.Pages.Replays
{
    public class MapEntryModel : PageModel
    {
        [FromForm(Name = "beatmap")]
        public string? Beatmap { get; set; }

        public (List<(string, int)>, DateTime) MostPlayedMaps;
        public const string DefaultBeatmap = "c2a034a5c6d3a7fec931e065f4b12a66";
        public readonly Dictionary<string, Beatmap> Maps = new();
        public bool InvalidBeatmap = false;
        public long TotalCount;

        private readonly BeatmapDataService beatmapDataService;
        private readonly ReplayDatabaseService replayDatabaseService;
        
        public MapEntryModel(ReplayDatabaseService replayDatabaseService, BeatmapDataService beatmapDataService)
        {
            this.beatmapDataService = beatmapDataService;
            this.replayDatabaseService = replayDatabaseService;
            MostPlayedMaps = (replayDatabaseService.MostPlayedMaps, replayDatabaseService.LastUpdated);
        }

        private async Task EnsureDataAvailable()
        {
            MostPlayedMaps = (replayDatabaseService.MostPlayedMaps, replayDatabaseService.LastUpdated);
            TotalCount = await beatmapDataService.CountBeatmap();
            var hashChunks = MostPlayedMaps.Item1
                .Select(pair => pair.Item1)
                .Chunk(6);

            foreach (var chunk in hashChunks)
            {
                var pairs = await Task.WhenAll(
                    chunk
                        .Select(async hash =>
                        {
                            var beatmap = await beatmapDataService.GetByHash(hash);
                            return (hash, beatmap);
                        }));

                foreach (var (hash, beatmap) in pairs)
                {
                    if (beatmap != null) Maps[hash] = beatmap;
                }
            }
        }
        
        public async Task OnGetAsync()
        {
            await EnsureDataAvailable();
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            Beatmap = Beatmap?.ToLowerInvariant();
            if (Beatmap != null)
            {
                var replayCount = await replayDatabaseService.Collection
                    .CountDocumentsAsync(
                        Builders<Replay>.Filter.Eq("beatmap_hash", Beatmap),
                        new CountOptions { Limit = 1 }
                    );

                if (replayCount == 0)
                {
                    InvalidBeatmap = true;
                    await EnsureDataAvailable();
                    return Page();
                }
            }
            
            return Redirect($"/replays/map/{Beatmap ?? DefaultBeatmap}");
        }
    }
}