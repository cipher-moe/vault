using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace vault.Entities
{
    public class Beatmap
    {
        [Column("md5")] public string BeatmapHash { get; set; } = "";
        [Column("beatmapId")] public int BeatmapId { get; set; }
        [Column("beatmapsetId")] public int BeatmapsetId  { get; set; }
        [Column("date")] public DateTime Timestamp  { get; set; }
    }
}