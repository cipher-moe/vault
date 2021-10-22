using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using OsuSharp;
using vault.Services;
using Replay = vault.Services.ReplayDatabase.Replay;

namespace vault.Pages.Replays
{
    public class ReplayMapModel : PageModel
    {
        private readonly ReplayDatabaseService service;
        private readonly BeatmapDataService beatmapDataService;
        public Beatmap? Map;
        public long TotalCount;
        public string Hash { get; set; } = "";
        public SortedDictionary<int, Replay[]> SortedReplays = new();
        public Replay[] Replays = Array.Empty<Replay>();
        public Dictionary<LegacyMods, long> TopModScores = new();

        public enum SortBy
        {
            Score = 0,
            Combo,
            Accuracy,
            Miss,
            Timestamp,
            Time = Timestamp
        }

        [FromQuery(Name = "sort")]
        public SortBy? Order { get; set; }


        public ReplayMapModel(ReplayDatabaseService service, BeatmapDataService beatmapDataService)
        {
            this.service = service;
            this.beatmapDataService = beatmapDataService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Hash = RouteData.Values["hash"]?.ToString()!;

            if (string.IsNullOrWhiteSpace(Hash))
            {
                return Redirect("/Maps/MostPlayed");
            }
            
            TotalCount = await service.Collection.EstimatedDocumentCountAsync();

            var sortDefinition = Order switch
            {
                SortBy.Timestamp or SortBy.Time => Builders<Replay>.Sort.Descending(replay => replay.Timestamp),
                SortBy.Combo => Builders<Replay>.Sort.Descending(replay => replay.MaxCombo),
                SortBy.Miss => Builders<Replay>.Sort.Ascending(replay => replay.CountMiss),
                SortBy.Accuracy => Builders<Replay>.Sort.Combine(
                    Builders<Replay>.Sort.Descending(replay => replay.Count300),
                    Builders<Replay>.Sort.Descending(replay => replay.Count100),
                    Builders<Replay>.Sort.Descending(replay => replay.Count50)
                ),
                _ => Builders<Replay>.Sort.Descending(replay => replay.Score)
            };
                
            var cursor = await service.Collection.FindAsync(
                Builders<Replay>.Filter.Eq("beatmap_hash", Hash),
                new FindOptions<Replay>
                {
                    Sort = sortDefinition
                }
            );
            var listing = (await cursor.ToListAsync())!;
            Replays = listing.ToArray();
            var groupedReplays = listing
                .GroupBy(replay => replay.Mode)
                .OrderBy(group => group.Key)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray()
                );

            SortedReplays = new SortedDictionary<int, Replay[]>(groupedReplays);
                
            if (Replays.Length != 0)
                Map = await beatmapDataService.GetByHash(Replays[0].BeatmapHash);

            foreach (var replay in Replays)
            {
                var score = new ScoreInfo
                {
                    Ruleset = ReplayRecentModel.Rulesets[replay.Mode].RulesetInfo,
                    RulesetID = replay.Mode,
                };
                score.SetCount50(replay.Count50);
                score.SetCount100(replay.Count100);
                score.SetCount300(replay.Count300);
                score.SetCountGeki(replay.CountGeki);
                score.SetCountKatu(replay.CountKatsu);
                score.SetCountMiss(replay.CountMiss);
                ReplayRecentModel.ScoreDecoder.CalculateAccuracy(score);
                replay.Accuracy = (score.Accuracy * 100).ToString("0.###");

                var mod = (LegacyMods)replay.Mods;
                TopModScores.TryGetValue(mod, out var topScore);
                if (topScore < replay.Score) TopModScores[mod] = replay.Score;
            }

            return Page();
        }
    }
}