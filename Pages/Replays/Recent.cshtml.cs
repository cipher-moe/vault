using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
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
using vault.Services;
using Beatmap = OsuSharp.Beatmap;
using Replay = vault.Services.ReplayDatabase.Replay;

namespace vault.Pages.Replays
{
    public class LegacyScoreDecoder : osu.Game.Scoring.Legacy.LegacyScoreDecoder
    {
        protected override WorkingBeatmap GetBeatmap(string md5Hash) => null;
        protected override Ruleset GetRuleset(int rulesetId) => ReplayRecentModel.Rulesets[rulesetId];
        public new void CalculateAccuracy(ScoreInfo score) => base.CalculateAccuracy(score);
    }
    
    public class ReplayRecentModel : PageModel
    {
        private readonly ReplayDatabaseService service;
        private readonly BeatmapDataService beatmapDataService;

        public long TotalCount = 0;
        public const int PageCount = 50;
        public List<Replay> Replays = new();
        public readonly Dictionary<string, Beatmap> Maps = new();

        [FromQuery(Name = "page")]
        public int PageIndex { get; set; } = 1;

        public ReplayRecentModel(ReplayDatabaseService service, BeatmapDataService beatmapDataService)
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
            Replays = await service.Collection.Find(FilterDefinition<Replay>.Empty)
                .SortByDescending(replay => replay.Timestamp)
                .Skip((PageIndex - 1) * PageCount)
                .Limit(PageCount)
                .ToListAsync();


            var hashChunks = Replays
                .Select(replay => replay.BeatmapHash)
                .Distinct()
                .Chunk(8);

            // parallelize the beatmap fetching to a certain degree
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

            foreach (var replay in Replays)
            {
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
                replay.Accuracy = (score.Accuracy * 100).ToString("0.###");
            }
        }
    }
}