using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using vault.Databases;
using Beatmap = OsuSharp.Beatmap;
using Replay = vault.Entities.Replay;

namespace vault.Pages.Replays
{
    public class ReplayMapModel : PageModel
    {
        private readonly ReplayDbContext replayDbContext;
        private readonly BeatmapDbContext beatmapDbContext;
        private readonly HttpClient httpClient;
        public Beatmap? Map;
        public WorkingBeatmap? WorkingBeatmap; 
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


        public ReplayMapModel(ReplayDbContext replayDbContext, BeatmapDbContext beatmapDbContext, HttpClient httpClient)
        {
            this.replayDbContext = replayDbContext;
            this.beatmapDbContext = beatmapDbContext;
            this.httpClient = httpClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Hash = RouteData.Values["hash"]?.ToString()!;

            if (string.IsNullOrWhiteSpace(Hash))
            {
                return Redirect("/Maps/MostPlayed");
            }

            TotalCount = await replayDbContext.Replays.CountAsync();

            var collection = replayDbContext.Replays.Where(r => r.BeatmapHash == Hash);
            
            var sortedCollection = Order switch
            {
                SortBy.Timestamp or SortBy.Time => collection.OrderByDescending(replay => replay.Timestamp),
                SortBy.Combo => collection.OrderByDescending(replay => replay.MaxCombo),
                SortBy.Miss => collection.OrderByDescending(replay => replay.CountMiss),
                SortBy.Accuracy => collection
                    .OrderByDescending(replay => replay.Count300)
                    .ThenByDescending(replay => replay.Count100)
                    .ThenByDescending(replay => replay.Count50),
                _ => collection.OrderByDescending(replay => replay.Score)
            };
            
            Replays = sortedCollection.ToArray();
            var groupedReplays = Replays
                .GroupBy(replay => replay.Mode)
                .OrderBy(group => group.Key)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToArray()
                );

            SortedReplays = new SortedDictionary<int, Replay[]>(groupedReplays);

            if (Replays.Length != 0)
            {
                Map = await beatmapDbContext.GetByHash(Replays[0].BeatmapHash);
                var mapfile = await httpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{Map!.BeatmapId}");
                WorkingBeatmap = Pepper.Commons.Osu.WorkingBeatmap.Decode(mapfile, (int?) Map?.BeatmapId);
            }

            foreach (var replay in Replays)
            {
                var score = new ScoreInfo
                {
                    Ruleset = ReplayRecentModel.Rulesets[replay.Mode].RulesetInfo
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