using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using GoodFriend.Client.Http;
using GoodFriend.Client.Http.Responses;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules;

/// <summary>
///     The base class for all modules.
/// </summary>
internal abstract class BaseModule
{
    /// <summary>
    ///     The PlayerEvent SSE stream.
    /// </summary>
    /// <remarks>
    ///    This is a convenience property for <see cref="Services.PlayerEventSseStream"/>.
    /// </remarks>
    protected static SseClient<PlayerEventStreamUpdate> PlayerEventSseStream => Services.PlayerEventSseStream;

    /// <summary>
    ///     The Announcement SSE stream.
    /// </summary>
    /// <remarks>
    ///    This is a convenience property for <see cref="Services.AnnouncementSseStream"/>.
    /// </remarks>
    protected static SseClient<AnnouncementStreamUpdate> AnnouncementSseStream => Services.AnnouncementSseStream;

    /// <summary>
    ///     The shared HttpClient.
    /// </summary>
    /// <remarks>
    ///    This is a convenience property for <see cref="Services.HttpClient"/>.
    /// </remarks>
    protected static HttpClient HttpClient => Services.HttpClient;

    /// <summary>
    ///     a name for this module.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     An optional description for this module.
    /// </summary>
    public virtual string? Description { get; }

    /// <summary>
    ///     The category tag for this module.
    /// </summary>
    public abstract ModuleTag Tag { get; }

    /// <summary>
    ///     The current state of this module.
    /// </summary>
    public ModuleState State { get; set; } = ModuleState.Disabled;

    /// <summary>
    ///     The priority of this module when loading. Higher numbers are loaded first.
    /// </summary>
    public virtual uint LoadPriority { get; }

    /// <summary>
    ///     The priority of this module when displaying it with others. Higher numbers are displayed first.
    /// </summary>
    public virtual uint DisplayWeight { get; }

    /// <summary>
    ///     Enables the module.
    /// </summary>
    protected abstract void OnEnable();

    /// <summary>
    ///     Enables the module.
    /// </summary>
    public void Enable()
    {
        try
        {
            Logger.Information($"Began loading module {this.GetType().FullName}...");
            this.State = ModuleState.Loading;

            this.OnEnable();
            this.State = ModuleState.Enabled;
            Logger.Information($"Loaded module {this.GetType().FullName}.");
        }
        catch (Exception e)
        {
            this.State = ModuleState.Error;
            Logger.Error($"Failed to load module {this.GetType().FullName}: {e}");
            try
            {
                this.OnDisable();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to unload module {this.GetType().FullName} after it errored: {ex}");
            }
        }
    }

    /// <summary>
    ///     Disables the module.
    /// </summary>
    protected abstract void OnDisable();

    /// <summary>
    ///     Disables the module.
    /// </summary>
    public void Disable()
    {
        if (this.State is ModuleState.Disabled)
        {
            Logger.Warning($"Not unloading module {this.GetType().FullName} as it is already disabled or is in an error state.");
            return;
        }

        try
        {
            this.State = ModuleState.Unloading;
            Logger.Information($"Unloading module {this.GetType().FullName}...");

            this.OnDisable();
            this.State = ModuleState.Disabled;
            Logger.Information($"Unloaded module {this.GetType().FullName}.");
        }
        catch (Exception e)
        {
            this.State = ModuleState.Error;
            Logger.Error($"Failed to unload module {this.GetType().FullName}: {e}");
        }
    }

    /// <summary>
    ///     Draws the module.
    /// </summary>
    public void Draw()
    {
        if (this.State is ModuleState.Error)
        {
            SiGui.TextWrappedColoured(Colours.Error, Strings.Modules_ModuleBase_CantDisplayError);
            return;
        }

        try
        {
            this.DrawModule();
        }
        catch (Exception e)
        {
            SiGui.TextColoured(Colours.Error, string.Format(Strings.Modules_ModuleBase_FailedDraw, this.GetType().FullName, e));
        }
    }

    /// <summary>
    ///     Called after the <see cref="DrawBase" /> method has finished drawing.
    /// </summary>
    /// <remarks>
    ///     This method should be used by the module itself to draw its contents.
    /// </remarks>
    protected virtual void DrawModule() => SiGui.TextDisabledWrapped(Strings.Modules_ModuleBase_NoAdditionalContent);
}

/// <summary>
///     The base class for all module configurations.
/// </summary>
internal abstract class BaseModuleConfig
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    ///     The version of the module configuration.
    /// </summary>
    public abstract uint Version { get; protected set; }

    /// <summary>
    ///     The module's unique identifier, used for identifying the module configuration file.
    /// </summary>
    /// <remarks>
    ///     Changing this identifier will lead to the module being unable to load its previous configuration.
    ///     You should migrate the configuration to the new identifier if this is changed.
    ///     Consider using <see cref="Rename" /> for this.
    /// </remarks>
    protected abstract string Identifier { get; set; }

    /// <summary>
    ///     Saves the given module configuration to disk.
    /// </summary>
    public void Save()
    {
        if (!Directory.Exists(Constants.Directory.ModuleConfig))
        {
            Directory.CreateDirectory(Constants.Directory.ModuleConfig);
        }
        var configJson = JsonSerializer.Serialize((object)this, JsonSerializerOptions);
        File.WriteAllText(Path.Combine(Constants.Directory.ModuleConfig, $"{this.Identifier}.json"), configJson);
    }

    /// <summary>
    ///     Renames the module configuration file from the old identifier to the new identifier.
    /// </summary>
    /// <param name="oldIdentifier">The old identifier.</param>
    /// <param name="newIdentifier">The new identifier.</param>
    public static void Rename(string oldIdentifier, string newIdentifier)
    {
        if (!Directory.Exists(Constants.Directory.ModuleConfig))
        {
            Directory.CreateDirectory(Constants.Directory.ModuleConfig);
        }
        var oldConfigPath = Path.Combine(Constants.Directory.ModuleConfig, $"{oldIdentifier}.json");
        var newConfigPath = Path.Combine(Constants.Directory.ModuleConfig, $"{newIdentifier}.json");
        if (File.Exists(oldConfigPath))
        {
            File.Move(oldConfigPath, newConfigPath);
        }
    }

    /// <summary>
    ///     Loads the given module configuration from disk.
    /// </summary>
    /// <typeparam name="T">The type of the module configuration to load.</typeparam>
    /// <returns>A module configuration either from disk or new if not found.</returns>
    public static T Load<T>() where T : BaseModuleConfig, new()
    {
        if (!Directory.Exists(Constants.Directory.ModuleConfig))
        {
            Directory.CreateDirectory(Constants.Directory.ModuleConfig);
        }

        var configPath = Path.Combine(Constants.Directory.ModuleConfig, $"{new T().Identifier}.json");

        if (!File.Exists(configPath))
        {
            return new T();
        }

        var configJson = File.ReadAllText(configPath);
        try
        {
            return JsonSerializer.Deserialize<T>(configJson) ?? new T();
        }
        catch (JsonException e)
        {
            Logger.Error($"Failed to deserialize module configuration {typeof(T).FullName}: {e}");
            return new T();
        }
    }
}

/// <summary>
///     The current state of the module.
/// </summary>
internal enum ModuleState
{
    /// <summary>
    ///     The module is currently enabled.
    /// </summary>
    Enabled,

    /// <summary>
    ///     The module is currently Loading.
    /// </summary>
    Loading,

    /// <summary>
    ///     The module is currently disabled.
    /// </summary>
    Disabled,

    /// <summary>
    ///     The module is currently Unloading   .
    /// </summary>
    Unloading,

    /// <summary>
    ///     The module has encountered an error and will not be usable.
    /// </summary>
    Error,
}

/// <summary>
///     The tag applied to this module for helping users understand its purpose.
/// </summary>
internal enum ModuleTag
{
    Information,
    Notifications,
    Connectivity,
}
