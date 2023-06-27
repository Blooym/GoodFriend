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
                Logger.Debug($"Began loading module {this.GetType().FullName}...");
                this.State = ModuleState.Loading;

                this.EnableAction();
                this.State = ModuleState.Enabled;
                Logger.Debug($"Loaded module {this.GetType().FullName}.");
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
                Logger.Debug($"Unloading module {this.GetType().FullName}...");

                this.DisableAction();
                this.State = ModuleState.Disabled;
                Logger.Debug($"Unloaded module {this.GetType().FullName}.");
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
        ///     Consider using <see cref="Rename_Experimental" /> for this.
        /// </remarks>
        protected abstract string Identifier { get; set; }

        /// <summary>
        ///     Creates the module directory if it does not exist.
        /// </summary>
        private static void EnsureModuleDirectory()
        {
            if (!Directory.Exists(Constants.Directory.ModuleConfig))
            {
                Directory.CreateDirectory(Constants.Directory.ModuleConfig);
            }
        }

        /// <summary>
        ///     Called when the configuration is loaded, can be used to run things like renames.
        /// </summary>
        protected virtual void OnConfigLoaded() { }

        /// <summary>
        ///     Saves the given module configuration to disk.
        /// </summary>
        public void Save()
        {
            EnsureModuleDirectory();
            var configJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(Constants.Directory.ModuleConfig, $"{this.Identifier}.json"), configJson);
        }

        /// <summary>
        ///     Renames the module configuration file from current identifier to the new identifier.
        /// </summary>
        /// <param name="newIdentifier">The new identifier.</param>
        /// <remarks>
        ///     This is a potentially dangerous action and should be used carefully.
        /// </remarks>
        public void Rename_Experimental(string newIdentifier)
        {
            EnsureModuleDirectory();

            Logger.Debug($"Running rename from {this.Identifier} -> {newIdentifier}");

            var oldConfigPath = Path.Combine(Constants.Directory.ModuleConfig, $"{this.Identifier}.json");
            var newConfigPath = Path.Combine(Constants.Directory.ModuleConfig, $"{newIdentifier}.json");
            if (File.Exists(oldConfigPath))
            {
                File.Move(oldConfigPath, newConfigPath, true);
            }

            this.Identifier = newIdentifier;
            this.Save();
        }

        /// <summary>
        ///     Loads the given module configuration from disk.
        /// </summary>
        /// <typeparam name="T">The type of the module configuration to load.</typeparam>
        /// <returns>A module configuration either from disk or new if not found.</returns>
        public static T Load<T>() where T : ModuleConfigBase, new()
        {
            EnsureModuleDirectory();

            var newConfig = new T();
            var configPath = Path.Combine(Constants.Directory.ModuleConfig, $"{newConfig.Identifier}.json");
            if (!File.Exists(configPath))
            {
                Logger.Debug($"Configuration for {newConfig.Identifier} does not exist, creating a new one.");
                newConfig.OnConfigLoaded();
                return newConfig;
            }

            var configJson = File.ReadAllText(configPath);
            var loadedConfig = JsonConvert.DeserializeObject<T>(configJson) ?? new T();
            loadedConfig.OnConfigLoaded();
            Logger.Debug($"Loaded configuration {loadedConfig.Identifier}");
            return loadedConfig;
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
        Experimental,
    }
}
