using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OsuSharp;
using vault.Databases;
using vault.Services;

namespace vault.Pages.Maps
{
    public class MostPlayedMapsModel : PageModel
    {
        [FromForm(Name = "beatmap")]
        public string? Beatmap { get; set; }

        public (List<(string, int)>, DateTime) MostPlayedMaps;
        public const string DefaultBeatmap = "c2a034a5c6d3a7fec931e065f4b12a66";
        public readonly Dictionary<string, Beatmap> Maps = new();
        public bool InvalidBeatmap;
        public long TotalCount;

        private readonly ReplayDbContext replayDbContext;
        private readonly BeatmapDbContext beatmapDbContext;
        private readonly MostPlayedMapsService mostPlayedMapsService;
        
        public MostPlayedMapsModel(ReplayDbContext replayDbContext, BeatmapDbContext beatmapDbContext, MostPlayedMapsService mostPlayedMapsService)
        {
            this.replayDbContext = replayDbContext;
            this.beatmapDbContext = beatmapDbContext;
            this.mostPlayedMapsService = mostPlayedMapsService;
        }

        private async Task EnsureDataAvailable()
        {
            MostPlayedMaps = (mostPlayedMapsService.MostPlayedMaps, mostPlayedMapsService.LastUpdated);
            TotalCount = await beatmapDbContext.Beatmaps.CountAsync();
            var hashChunks = MostPlayedMaps.Item1
                .Select(pair => pair.Item1)
                .Chunk(6);

            foreach (var chunk in hashChunks)
            {
                var pairs = await Task.WhenAll(
                    chunk
                        .Select(async hash =>
                        {
                            var beatmap = await beatmapDbContext.GetByHash(hash);
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
                var replayCount = await replayDbContext.Replays.Where(r => r.BeatmapHash == Beatmap).CountAsync();

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