using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Responses
{
    [Serializable]
    public readonly record struct EventStreamPlayerUpdate
    {
        [JsonPropertyName("content_id_hash")]
        public readonly string ContentIdHash { get; init; }

        [JsonPropertyName("content_id_salt")]
        public readonly string ContentIdSalt { get; init; }

        [JsonPropertyName("datacenter_id")]
        public readonly uint DatacenterId { get; init; }

        [JsonPropertyName("world_id")]
        public readonly uint WorldId { get; init; }

        [JsonPropertyName("territory_id")]
        public readonly uint TerritoryId { get; init; }

        [JsonPropertyName("is_logged_in")]
        public readonly bool IsLoggedIn { get; init; }
    }
}