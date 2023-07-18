using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Responses
{
    /// <summary>
    ///     Represents a response from the player event stream.
    /// </summary>
    [Serializable]
    public record struct PlayerEventStreamUpdate
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
        ///     The state update type of the event.
        /// </summary>
        [JsonPropertyName("state_update_type")]
        public StateUpdateType StateUpdateType { get; set; }
    }

    /// <summary>
    ///     Represents the type of state update received.
    /// </summary>
    [Serializable]
    public record struct StateUpdateType
    {
        /// <summary>
        ///     Represents the data received relating to a login state change.
        /// </summary>
        [JsonPropertyName("LoginStateChange")]
        public LoginStateChangeData? LoginStateChange { get; set; }

        /// <summary>
        ///     Represents the data received relating to a world change.
        /// </summary>
        [JsonPropertyName("WorldChange")]
        public WorldChangeData? WorldChange { get; set; }
    }

    /// <summary>
    ///     Data for the LoginStateChange event.
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

    /// <summary>
    ///     Data for the WorldChange event.
    /// </summary>
    [Serializable]
    public record struct WorldChangeData
    {
        /// <summary>
        ///     The ID of the world the player moved to.
        /// </summary>
        [JsonPropertyName("world_id")]
        public uint WorldId { get; set; }
    }
}
