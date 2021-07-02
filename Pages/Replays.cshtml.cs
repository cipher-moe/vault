using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public Replay[] Replays = Array.Empty<Replay>();
        public Beatmap Beatmap;

        public ReplayModel(ReplayDatabaseService service)
        {
            this.service = service;
        }

        public static OsuClient APIClient = new (new OsuSharpConfiguration
        {
            ApiKey = Environment.GetEnvironmentVariable("OSU_API_KEY")
        });

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
            var hash = RouteData.Values["hash"]?.ToString();
            if (!string.IsNullOrWhiteSpace(hash))
            {
                Replays = service.Collection.FindSync(Builders<Replay>.Filter.Eq("beatmap_hash", hash))
                    .ToList().ToArray();
                Beatmap = await APIClient.GetBeatmapByHashAsync(Replays[0].BeatmapHash);
            }
            else
                Replays = service.Collection.Find(FilterDefinition<Replay>.Empty)
                    .Limit(50)
                    .SortByDescending(replay => replay.Timestamp)
                    .ToList().ToArray();
        }
    }
}