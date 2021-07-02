using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vault.Services.ReplayDatabase
{
    public class Replay
    {
        [BsonElement("_id")] [BsonId] public ObjectId BsonDocumentId;
        [BsonElement("name")] public int Mode;
        [BsonElement("version")] public int Version;
        [BsonElement("beatmap_hash")] public string BeatmapHash = "";
        [BsonElement("player_name")] public string PlayerName = "";
        [BsonElement("replay_hash")] public string ReplayHash = "";
        [BsonElement("count_300")] public int Count300;
        [BsonElement("count_100")] public int Count100;
        [BsonElement("count_50")] public int Count50;
        [BsonElement("count_geki")] public int CountGeki;
        [BsonElement("count_katsu")] public int CountKatsu;
        [BsonElement("count_miss")] public int CountMiss;
        [BsonElement("score")] public long Score;
        [BsonElement("max_combo")] public int MaxCombo;
        [BsonElement("perfect_combo")] public bool PerfectCombo;
        [BsonElement("mods")] public long Mods;
        [BsonElement("timestamp")] public string Timestamp = "";
        [BsonElement("sha256")] public string Sha256 = "";
    }
}