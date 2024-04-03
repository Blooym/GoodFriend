using System;
using System.Reflection;

namespace GoodFriend.Plugin.Base;

/// <summary>
///     A collection of constant values used throughout the plugin.
/// </summary>
internal static class Constants
{
    /// <summary>
    ///     The plugin name.
    /// </summary>
    public const string PluginName = "GoodFriend";

    public const uint WaitingForFriendListApproval = 0x20;

    /// <summary>
    ///     Plugin build information.
    /// </summary>
    internal static class Build
    {
        /// <summary>
        ///     The version of the plugin.
        /// </summary>
        public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

        /// <summary>
        ///     The version of the plugin as a string.
        /// </summary>
        public static readonly string VersionInformational = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";

        /// <summary>
        ///     The build configuration that was used to compile the plugin.
        /// </summary>
        public static readonly string BuildConfiguration = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";

        /// <summary>
        ///     The client key used for this plugin.
        /// </summary>
        public static readonly string? ClientKey = Assembly.GetExecutingAssembly().GetCustomAttribute<ClientKeyAttribute>()?.Value;

    }

    /// <summary>
    ///     Links to various places.
    /// </summary>
    internal static class Link
    {
        /// <summary>
        ///     The URL to the donation page.
        /// </summary>
        public const string Donate = "https://go.blooym.dev/donate";

        /// <summary>
        ///     The URL to sponsor the project.
        /// </summary>
        public const string Sponsors = "https://go.blooym.dev/sponsor";
    }

    /// <summary>
    ///     The plugin's directories.
    /// </summary>
    internal static class Directory
    {
        /// <summary>
        ///     The directory where the module configurations are stored.
        /// </summary>
        public static readonly string ModuleConfig = @$"{DalamudInjections.PluginInterface.ConfigDirectory}\ModuleConfig\";
    }
}

/// <summary>
///     The client key used for this plugin.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
internal sealed class ClientKeyAttribute(string value) : Attribute
{
    public string Value { get; set; } = value;
}
