using System;
using System.Reflection;

namespace GoodFriend.Plugin.Common
{
    /// <summary>
    ///     A collection of translatable strings for the plugin.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        ///     The plugin's name.
        /// </summary>
        internal const string PluginName = "GoodFriend";

        /// <summary>
        ///     Plugin build information.
        /// </summary>
        internal static class Build
        {
            /// <summary>
            ///     The version of the plugin.
            /// </summary>
            internal static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

            /// <summary>
            ///     The version of the plugin as a string.
            /// </summary>
            internal static readonly string VersionInformational = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Unknown";

            /// <summary>
            ///     The last commit hash when the plugin was compiled.
            /// </summary>
            internal static readonly string GitCommitHash = Assembly.GetExecutingAssembly().GetCustomAttribute<GitHashAttribute>()?.Value ?? "Unknown";

            /// <summary>
            ///     The last commit message when the plugin was compiled.
            /// </summary>
            internal static readonly string GitCommitMessage = Assembly.GetExecutingAssembly().GetCustomAttribute<GitCommitMessageAttribute>()?.Value ?? "Unknown";

            /// <summary>
            ///     The date of the last commit when the plugin was compiled.
            /// </summary>
            internal static readonly DateTime GitCommitDate = DateTime.TryParse(Assembly.GetExecutingAssembly().GetCustomAttribute<GitCommitDateAttribute>()?.Value, out var date) ? date : DateTime.MinValue;

            /// <summary>
            ///     The branch that was active when the plugin was compiled.
            /// </summary>
            internal static readonly string GitBranch = Assembly.GetExecutingAssembly().GetCustomAttribute<GitBranchAttribute>()?.Value ?? "Unknown";

            /// <summary>
            ///     The build configuration that was used to compile the plugin.
            /// </summary>
            internal static readonly string BuildConfiguration = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration ?? "Unknown";

            /// <summary>
            ///     Whether or not the build is a pre-release / development build.
            /// </summary>
            /// <remarks>
            ///     Checks for the following to determine if the build is a pre-release:
            ///     <list type="bullet">
            ///         <item>Does the plugin manifest indicate that this is a testing-only release?</item>
            ///         <item>Was the build configuration set to "Debug"?</item>
            ///         <item>Does the version contain "alpha"?</item>
            ///         <item>Does the version contain "beta"?</item>
            ///         <item>Does the version contain "rc"?</item>
            ///     </list>
            /// </remarks>
            internal static readonly bool IsPreRelease =
                Services.PluginInterface.IsTesting ||
                Services.PluginInterface.IsDev ||
                BuildConfiguration.Equals("Debug", StringComparison.OrdinalIgnoreCase) ||
                VersionInformational.Contains("alpha") ||
                VersionInformational.Contains("beta") ||
                VersionInformational.Contains("rc");
        }

        /// <summary>
        ///     Links to various places.
        /// </summary>
        internal static class Links
        {
            internal const string KoFi = "https://ko-fi.com/BitsOfAByte";
            internal const string GitHubSponsors = "https://github.com/sponsors/BitsOfAByte";
            internal static readonly Uri DefaultAPIUrl = new("https://aether.bitsofabyte.dev/");
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