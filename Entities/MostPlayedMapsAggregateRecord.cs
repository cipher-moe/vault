using System.ComponentModel.DataAnnotations.Schema;

namespace vault.Entities
{
    public class MostPlayedMapsAggregateRecord
    {
        [Column("beatmap_hash")] public string BeatmapHash { get; set; }
        [Column("count")] public int Count { get; set; }
    }
}