using System;
using System.Text.Json.Serialization;

namespace GoodFriend.Client.Types
{
    /// <summary>
    ///     Represents a game version from ffxivgame.ver.
    /// </summary>
    [Serializable]
    public readonly record struct GameVersion
    {
        /// <summary>
        ///     The year of the game version.
        /// </summary>
        [JsonPropertyName("year")]
        public readonly string Year { get; init; }

        /// <summary>
        ///     The month of the game version.
        /// </summary>
        [JsonPropertyName("month")]
        public readonly string Month { get; init; }

        /// <summary>
        ///     The day of the game version.
        /// </summary>
        [JsonPropertyName("day")]
        public readonly string Day { get; init; }

        /// <summary>
        ///     The major version of the game version.
        /// </summary>
        [JsonPropertyName("major")]
        public readonly string Major { get; init; }

        /// <summary>
        ///     The minor version of the game version.
        /// </summary>
        [JsonPropertyName("minor")]
        public readonly string Minor { get; init; }

        /// <summary>
        ///     Converts the given game version to an integer.
        /// </summary>
        /// <returns>An integer of the GameVersion</returns>
        private int ToInt() => int.Parse($"{this.Year}{this.Month}{this.Day}{this.Major}{this.Minor}");

        /// <summary>
        ///     Converts a string to a GameVersion.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <returns>The GameVersion.</returns>
        public static GameVersion FromString(string version)
        {
            var split = version.Trim().Split('.');
            return new GameVersion
            {
                Year = split[0],
                Month = split[1],
                Day = split[2],
                Major = split[3],
                Minor = split[4]
            };
        }

        /// <summary>
        ///     Tries to convert a string to a GameVersion.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="gameVersion">The GameVersion.</param>
        /// <returns>True if the conversion was successful, false otherwise.</returns>
        public static bool TryFromString(string version, out GameVersion gameVersion)
        {
            try
            {
                gameVersion = FromString(version);
                return true;
            }
            catch (Exception)
            {
                gameVersion = default;
                return false;
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"{this.Year}.{this.Month}.{this.Day}.{this.Major}.{this.Minor}";

        /// <inheritdoc />
        public static bool operator >(GameVersion a, GameVersion b) => a.ToInt() > b.ToInt();

        /// <inheritdoc />
        public static bool operator <(GameVersion a, GameVersion b) => a.ToInt() < b.ToInt();

        /// <inheritdoc />
        public static bool operator >=(GameVersion a, GameVersion b) => a.ToInt() >= b.ToInt();

        /// <inheritdoc />
        public static bool operator <=(GameVersion a, GameVersion b) => a.ToInt() <= b.ToInt();
    }
}