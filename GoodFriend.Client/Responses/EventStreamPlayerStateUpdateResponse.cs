using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Responses
{
    /// <summary>
    ///     Represents a response from the player state event stream.
    /// </summary>
    [Serializable]
    public record struct EventStreamPlayerStateUpdateResponse
    {
        /// <summary>
        ///     The ContentID Hash of the player.
        /// </summary>
        [JsonPropertyName("content_id_hash")]
        public string ContentIdHash { get; set; }

        /// <summary>
        ///     The salt used when hashing the player's ContentID.
        /// </summary>
        [JsonPropertyName("content_id_salt")]
        public string ContentIdSalt { get; set; }

        /// <summary>
        ///     Represents the data recieved relating to a <see cref="LoginStateChangeData" /> event.
        /// </summary>
        [JsonPropertyName("LoginStateChange")]
        public LoginStateChangeData? LoginStateChange { get; set; }
    }

    /// <summary>
    ///     Represents a LoginStateChange event.
    /// </summary>
    [Serializable]
    public record struct LoginStateChangeData
    {
        [JsonPropertyName("datacenter_id")]
        public uint DatacenterId { get; set; }

        [JsonPropertyName("world_id")]
        public uint WorldId { get; set; }

        [JsonPropertyName("territory_id")]
        public ushort TerritoryId { get; set; }

        [JsonPropertyName("logged_in")]
        public bool LoggedIn { get; set; }
    }
}
