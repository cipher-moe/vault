using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OsuSharp;
using vault.Services;

namespace vault.Pages.Replays
{
    public class MapEntryModel : PageModel
    {
        [FromForm(Name = "beatmap")]
        public string? Beatmap { get; set; }

        public readonly List<(string, int)> MostPlayedMaps;
        public const string DefaultBeatmap = "c2a034a5c6d3a7fec931e065f4b12a66";
        public readonly Dictionary<string, Beatmap> Maps = new();

        private readonly BeatmapDataService beatmapDataService;
        
        public MapEntryModel(ReplayDatabaseService replayDatabaseService, BeatmapDataService beatmapDataService)
        {
            this.beatmapDataService = beatmapDataService;
            MostPlayedMaps = replayDatabaseService.MostPlayedMaps;
        }

        public async Task OnGetAsync()
        {
            var hashChunks = MostPlayedMaps
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
        
        public IActionResult OnPost()
        {
            return Redirect($"/replays/map/{Beatmap ?? DefaultBeatmap}");
        }
    }
}