using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
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
        public Replay[] Replays = Array.Empty<Replay>();
        public Dictionary<LegacyMods, long> TopModScores = new();

        public ReplayMapModel(ReplayDatabaseService service, BeatmapDataService beatmapDataService)
        {
            this.service = service;
            this.beatmapDataService = beatmapDataService;
        }

        public async Task OnGetAsync()
        {
            TotalCount = await service.Collection.EstimatedDocumentCountAsync();
            Hash = RouteData.Values["hash"]?.ToString()!;
            Replays = service.Collection.FindSync(Builders<Replay>.Filter.Eq("beatmap_hash", Hash))
                .ToList()
                .OrderByDescending(replay => replay.Score)
                .ToArray();
            if (Replays.Length != 0)
                Map = await beatmapDataService.GetByHash(Replays[0].BeatmapHash);

            foreach (var replay in Replays)
            {
                var score = new ScoreInfo
                {
                    Ruleset = ReplayListModel.Rulesets[replay.Mode].RulesetInfo,
                    RulesetID = replay.Mode,
                };
                score.SetCount50(replay.Count50);
                score.SetCount100(replay.Count100);
                score.SetCount300(replay.Count300);
                score.SetCountGeki(replay.CountGeki);
                score.SetCountKatu(replay.CountKatsu);
                score.SetCountMiss(replay.CountMiss);
                ReplayListModel.ScoreDecoder.CalculateAccuracy(score);
                replay.Accuracy = (score.Accuracy * 100).ToString("F3");

                var mod = (LegacyMods)replay.Mods;
                TopModScores.TryGetValue(mod, out var topScore);
                if (topScore < replay.Score) TopModScores[mod] = replay.Score;
            }
        }
    }
}