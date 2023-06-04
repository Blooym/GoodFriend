using System;
using System.IO;
using GoodFriend.Client;
using GoodFriend.Plugin.Base;
using Newtonsoft.Json;
using Sirensong.UserInterface;
using Sirensong.UserInterface.Style;

namespace GoodFriend.Plugin.Api.ModuleSystem
{
    /// <summary>
    ///     The base class for all API modules.
    /// </summary>
    internal abstract class ApiModuleBase
    {
        /// <summary>
        ///     The API client instance.
        /// </summary>
        protected static GoodFriendClient ApiClient => RelayApiService.ApiClientInstance;

        /// <summary>
        ///     A user-friendly name for this module.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     The tag for this module to help categorize it.
        /// </summary>
        public abstract ApiModuleTag Tag { get; }

        /// <summary>
        ///     The current state of this module.
        /// </summary>
        public ApiModuleState State { get; set; } = ApiModuleState.Disabled;

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
                this.State = ApiModuleState.Loading;

                this.EnableAction();
                this.State = ApiModuleState.Enabled;
                Logger.Information($"Loaded module {this.GetType().FullName}.");
            }
            catch (Exception e)
            {
                this.State = ApiModuleState.Error;
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
            if (this.State is ApiModuleState.Disabled)
            {
                Logger.Warning($"Not unloading module {this.GetType().FullName} as it is already disabled or is in an error state.");
                return;
            }

            try
            {
                this.State = ApiModuleState.Unloading;
                Logger.Information($"Unloading module {this.GetType().FullName}...");

                this.DisableAction();
                this.State = ApiModuleState.Disabled;
                Logger.Information($"Unloaded module {this.GetType().FullName}.");
            }
            catch (Exception e)
            {
                this.State = ApiModuleState.Error;
                Logger.Error($"Failed to unload module {this.GetType().FullName}: {e}");
            }
        }

        /// <summary>
        ///     Draws the module.
        /// </summary>
        public void Draw()
        {
            if (this.State is ApiModuleState.Error)
            {
                SiGui.TextWrappedColoured(Colours.Error, "This module has encountered an error while running and was disabled. Please check the logs for more information.");
                return;
            }

            try
            {
                this.DrawBase();
            }
            catch (Exception e)
            {
                SiGui.TextColoured(Colours.Error, $"Failed to draw module {this.GetType().FullName}: {e}");
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
    internal abstract class ApiModuleConfigBase
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
            if (!Directory.Exists(Constants.Directory.ApiModuleConfig))
            {
                Directory.CreateDirectory(Constants.Directory.ApiModuleConfig);
            }
            var configJson = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(Path.Combine(Constants.Directory.ApiModuleConfig, $"{this.Identifier}.json"), configJson);
        }

        /// <summary>
        ///     Renames the module configuration file from the old identifier to the new identifier.
        /// </summary>
        /// <param name="oldIdentifier">The old identifier.</param>
        /// <param name="newIdentifier">The new identifier.</param>
        public static void Rename(string oldIdentifier, string newIdentifier)
        {
            if (!Directory.Exists(Constants.Directory.ApiModuleConfig))
            {
                Directory.CreateDirectory(Constants.Directory.ApiModuleConfig);
            }
            var oldConfigPath = Path.Combine(Constants.Directory.ApiModuleConfig, $"{oldIdentifier}.json");
            var newConfigPath = Path.Combine(Constants.Directory.ApiModuleConfig, $"{newIdentifier}.json");
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
        public static T Load<T>() where T : ApiModuleConfigBase, new()
        {
            if (!Directory.Exists(Constants.Directory.ApiModuleConfig))
            {
                Directory.CreateDirectory(Constants.Directory.ApiModuleConfig);
            }

            var configPath = Path.Combine(Constants.Directory.ApiModuleConfig, $"{new T().Identifier}.json");

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
    internal enum ApiModuleState
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
    internal enum ApiModuleTag
    {
        /// <summary>
        ///     A module that provides information to the user.
        /// </summary>
        Information,

        /// <summary>
        ///     A module that facilitates event sharing between friends.
        /// </summary>
        Friends,
    }
}