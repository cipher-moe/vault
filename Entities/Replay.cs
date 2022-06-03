using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace vault.Entities
{
    public class Replay
    {
        [Column("mode")] public int Mode { get; set; }
        [Column("version")] public int Version { get; set; }
        [Column("beatmap_hash")] public string BeatmapHash { get; set; } = "";
        [Column("player_name")] public string PlayerName { get; set; } = "";
        [Column("replay_hash")] public string? ReplayHash { get; set; } = "";
        [Column("count_300")] public int Count300 { get; set; }
        [Column("count_100")] public int Count100 { get; set; }
        [Column("count_50")] public int Count50 { get; set; }
        [Column("count_geki")] public int CountGeki { get; set; }
        [Column("count_katsu")] public int CountKatsu { get; set; }
        [Column("count_miss")] public int CountMiss { get; set; }
        [Column("score")] public long Score { get; set; }
        [Column("max_combo")] public int MaxCombo { get; set; }
        [Column("perfect_combo")] public bool PerfectCombo { get; set; }
        [Column("mods")] public long Mods { get; set; }
        [Column("timestamp")] public DateTime Timestamp { get; set; }
        [Column("sha256")] public string Sha256 { get; set; } = "";
        public string Accuracy = "";
    }
}