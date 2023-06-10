using System;
using System.IO;
using GoodFriend.Client;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.Localization;
using Newtonsoft.Json;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.ModuleSystem.Modules
{
    /// <summary>
    ///     The base class for all modules.
    /// </summary>
    internal abstract class ModuleBase
    {
        /// <summary>
        ///     The API client instance shared between all modules.
        /// </summary>
        /// <remarks>
        ///    This is a convenience property for <see cref="Services.GoodFriendApiClient" />.
        /// </remarks>
        protected static GoodFriendClient ApiClient => Services.GoodFriendApiClient;

        /// <summary>
        ///     A user-friendly name for this module.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     A user-friendly optional description for this module.
        /// </summary>
        public virtual string? Description { get; }

        /// <summary>
        ///     The tag for this module to help categorize it.
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
        /// <returns>True if the module was enabled successfully, otherwise false.</returns>
        protected abstract void EnableAction();

        /// <summary>
        ///     Enables the module.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the module is already enabled or is in an error state.</exception>
        public void Enable()
        {
            try
            {
                Logger.Information($"Began loading module {this.GetType().FullName}...");
                this.State = ModuleState.Loading;

                this.EnableAction();
                this.State = ModuleState.Enabled;
                Logger.Information($"Loaded module {this.GetType().FullName}.");
            }
            catch (Exception e)
            {
                this.State = ModuleState.Error;
                Logger.Error($"Failed to load module {this.GetType().FullName}: {e}");
                try
                {
                    this.DisableAction();
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
        /// <returns>True if the module was disabled successfully, otherwise false.</returns>
        protected abstract void DisableAction();

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

                this.DisableAction();
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
                this.DrawBase();
            }
            catch (Exception e)
            {
                SiGui.TextColoured(Colours.Error, string.Format(Strings.Modules_ModuleBase_FailedDraw, this.GetType().FullName, e));
            }
        }

        /// <summary>
        ///     Called after the <see cref="Draw" /> method has finished drawing.
        /// </summary>
        /// <remarks>
        ///     By default this will directly call <see cref="DrawModule" />
        ///     This method should be used by any upper abstract classes to pre-draw their contents before the module itself.
        ///     However, the module can choose to override this method to ignore that behaviour.
        /// </remarks>
        protected virtual void DrawBase() => this.DrawModule();

        /// <summary>
        ///     Called after the <see cref="DrawBase" /> method has finished drawing.
        /// </summary>
        /// <remarks>
        ///     This method should be used by the module itself to draw its contents.
        /// </remarks>
        protected virtual void DrawModule()
        {

        }
    }

    /// <summary>
    ///     The base class for all module configurations.
    /// </summary>
    internal abstract class ModuleConfigBase
    {
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
            var configJson = JsonConvert.SerializeObject(this, Formatting.Indented);
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
        public static T Load<T>() where T : ModuleConfigBase, new()
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
            return JsonConvert.DeserializeObject<T>(configJson) ?? new T();
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
        Connectivity,
        Notifications,
    }
}