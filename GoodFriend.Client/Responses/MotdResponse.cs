using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Responses
{
    [Serializable]
    public readonly struct MotdResponse
    {
        /// <summary>
        ///     The text content of the "message of the day".
        /// </summary>
        [JsonPropertyName("content")]
        public readonly string Content { get; init; }

        /// <summary>
        ///     The urgency string to be parsed as an enum.
        /// </summary>
        [JsonPropertyName("urgency"), JsonInclude]
        public readonly string BackingUrgencyString { get; init; }

        /// <summary>
        ///     The urgency string to be parsed as an enum.
        /// </summary>
        [JsonIgnore]
        public MotdUrgency Urgency => Enum.Parse<MotdUrgency>(this.BackingUrgencyString);

        /// <summary>
        ///     The brand string to be parsed as an enum.
        /// </summary>
        [JsonPropertyName("brand"), JsonInclude]
        public readonly string BackingBrandString { get; init; }

        /// <summary>
        ///     The brand, or "type", of message.
        /// </summary>
        [JsonIgnore]
        public MotdBrand Brand => Enum.Parse<MotdBrand>(this.BackingBrandString);

        /// <summary>
        ///     Whether or not the api wants the client to ignore this message.
        /// </summary>
        [JsonPropertyName("ignore")]
        public readonly bool Ignore { get; init; }
    }

    public enum MotdUrgency
    {
        /// <summary>
        ///     The lowest tier of urgency.
        /// </summary>
        Information = 0,

        /// <summary>
        ///     A medium tier of urgency for semi-critical messages.
        /// </summary>
        Warning = 2,

        /// <summary>
        ///     Critical urgency for only the most important of messages.
        /// </summary>
        Critical = 3,
    }

    public enum MotdBrand
    {
        /// <summary>
        ///     Maintenance type messages.
        /// </summary>
        Maintenance = 0,

        /// <summary>
        /// Issue type messages such as ongoing connectivity problems.
        /// </summary>
        Issue = 1,

        /// <summary>
        ///     A general, non-specific type of message.
        /// </summary>
        General = 2,
    }
}