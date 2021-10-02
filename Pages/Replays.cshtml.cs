using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using osu.Game.Beatmaps;
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
using Beatmap = OsuSharp.Beatmap;
using Replay = vault.Services.ReplayDatabase.Replay;

namespace vault.Pages
{
    public class LegacyScoreDecoder : osu.Game.Scoring.Legacy.LegacyScoreDecoder
    {
        protected override WorkingBeatmap GetBeatmap(string md5Hash) => null;
        protected override Ruleset GetRuleset(int rulesetId) => ReplayModel.Rulesets[rulesetId];
        public new void CalculateAccuracy(ScoreInfo score) => base.CalculateAccuracy(score);
    }
    
    public class ReplayModel : PageModel
    {
        private readonly ReplayDatabaseService service;
        private BeatmapDataService beatmapDataService;

        public long TotalCount = 0;
        public const int PageCount = 50;
        public Replay[] Replays = Array.Empty<Replay>();
        public readonly Dictionary<string, Beatmap> Maps = new();

        [FromQuery(Name = "page")]
        public int PageIndex { get; set; } = 1;
        
        public string Hash { get; set; }

        public ReplayModel(ReplayDatabaseService service, BeatmapDataService beatmapDataService)
        {
            this.service = service;
            this.beatmapDataService = beatmapDataService;
        }
        
        public static readonly LegacyScoreDecoder ScoreDecoder = new ();
        public static readonly Ruleset[] Rulesets = { new OsuRuleset(), new TaikoRuleset(), new CatchRuleset(), new ManiaRuleset() };
        public static IEnumerable<Mod> ModsFromModbits(Ruleset ruleset, LegacyMods mods)
        {
            if ((mods & LegacyMods.Nightcore) == LegacyMods.Nightcore)
                mods |= LegacyMods.DoubleTime;
            return ruleset.ConvertFromLegacyMods(mods);
        }
        
        public async Task OnGetAsync()
        {
            TotalCount = await service.Collection.EstimatedDocumentCountAsync();
            var hash = RouteData.Values["hash"]?.ToString();
            if (!string.IsNullOrWhiteSpace(hash))
            {
                Replays = service.Collection.FindSync(Builders<Replay>.Filter.Eq("beatmap_hash", hash))
                    .ToList().ToArray();
                if (Replays.Length != 0)
                    Maps[Replays[0].BeatmapHash] = await beatmapDataService.GetByHash(Replays[0].BeatmapHash);
            }
            else
            {
                Replays = service.Collection.Find(FilterDefinition<Replay>.Empty)
                    .SortByDescending(replay => replay.Timestamp)
                    .Skip((PageIndex - 1) * PageCount)
                    .Limit(PageCount)
                    .ToList()
                    .ToArray();
            }

            foreach (var replay in Replays)
            {
                Maps[replay.BeatmapHash] = await beatmapDataService.GetByHash(replay.BeatmapHash);
                var score = new ScoreInfo
                {
                    Ruleset = Rulesets[replay.Mode].RulesetInfo,
                    RulesetID = replay.Mode,
                };
                score.SetCount50(replay.Count50);
                score.SetCount100(replay.Count100);
                score.SetCount300(replay.Count300);
                score.SetCountGeki(replay.CountGeki);
                score.SetCountKatu(replay.CountKatsu);
                score.SetCountMiss(replay.CountMiss);
                ScoreDecoder.CalculateAccuracy(score);
                replay.Accuracy = (score.Accuracy * 100).ToString("F3");
            }
        }
    }
}