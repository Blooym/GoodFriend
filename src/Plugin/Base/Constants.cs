using System;
using System.Reflection;

namespace GoodFriend.Plugin.Base
{
    /// <summary>
    ///     A collection of constant values used throughout the plugin.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        ///     The plugin name.
        /// </summary>
        public const string PluginName = "GoodFriend";

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
            ///     The last commit hash when the plugin was compiled.
            /// </summary>
            public static readonly string GitCommitHash = Assembly.GetExecutingAssembly().GetCustomAttribute<GitHashAttribute>()?.Value ?? "Unknown";

            /// <summary>
            ///     The last commit message when the plugin was compiled.
            /// </summary>
            public static readonly string GitCommitMessage = Assembly.GetExecutingAssembly().GetCustomAttribute<GitCommitMessageAttribute>()?.Value ?? "Unknown";

            /// <summary>
            ///     The date of the last commit when the plugin was compiled.
            /// </summary>
            public static readonly DateTime GitCommitDate = DateTime.TryParse(Assembly.GetExecutingAssembly().GetCustomAttribute<GitCommitDateAttribute>()?.Value, out var date) ? date : DateTime.MinValue;

            /// <summary>
            ///     The branch that was active when the plugin was compiled.
            /// </summary>
            public static readonly string GitBranch = Assembly.GetExecutingAssembly().GetCustomAttribute<GitBranchAttribute>()?.Value ?? "Unknown";

            /// <summary>
            ///     The build configuration that was used to compile the plugin.
            /// </summary>
            public static readonly string BuildConfiguration = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";
        }

        /// <summary>
        ///     Links to various places.
        /// </summary>
        internal static class Link
        {
            /// <summary>
            ///     The URL to the donation page.
            /// </summary>
            public const string Donate = "https://bitsofabyte.dev/donate?utm_source=GoodFriend";

            /// <summary>
            ///     The URL to sponsor the project.
            /// </summary>
            public const string Sponsors = "https://bitsofabyte.dev/sponsor?utm_source=GoodFriend";
        }

        /// <summary>
        ///     The plugin's directories.
        /// </summary>
        internal static class Directory
        {
            /// <summary>
            ///     The directory where the api module configurations are stored.
            /// </summary>
            public static readonly string ApiModuleConfig = @$"{DalamudInjections.PluginInterface.ConfigDirectory}\ModuleConfig\";
        }
    }

    /// <summary>
    ///     The Git commit hash of the build.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    internal sealed class GitHashAttribute : Attribute
    {
        public GitHashAttribute(string value) => this.Value = value;
        public string Value { get; set; }
    }

    /// <summary>
    ///     The Git commit date of the build.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    internal sealed class GitCommitDateAttribute : Attribute
    {
        public GitCommitDateAttribute(string value) => this.Value = value;
        public string Value { get; set; }
    }

    /// <summary>
    ///     The Git branch of the build.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    internal sealed class GitBranchAttribute : Attribute
    {
        public GitBranchAttribute(string value) => this.Value = value;
        public string Value { get; set; }
    }

    /// <summary>
    ///     The Git commit message of the build.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    internal sealed class GitCommitMessageAttribute : Attribute
    {
        public GitCommitMessageAttribute(string value) => this.Value = value;
        public string Value { get; set; }
    }
}
