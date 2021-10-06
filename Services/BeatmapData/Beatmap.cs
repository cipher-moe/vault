using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace vault.Services.BeatmapData
{
    public class Beatmap
    {
        [BsonElement("_id")] [BsonId] public ObjectId BsonDocumentId;
        [BsonElement("md5")] public string BeatmapHash = "";
        [BsonElement("beatmapId")] public int BeatmapId;
        [BsonElement("beatmapsetId")] public int BeatmapsetId;
        [BsonElement("createdAt")] public DateTime Timestamp;
    }
}