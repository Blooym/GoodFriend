using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using GoodFriend.Client;
using GoodFriend.Plugin.Api.ModuleSystem;
using GoodFriend.Plugin.Base;

namespace GoodFriend.Plugin.Api
{
    internal sealed class RelayApiService : IDisposable
    {
        /// <summary>
        ///     All loaded modules.
        /// </summary>
        private readonly ICollection<ApiModuleBase> loadedModules;

        /// <summary>
        ///     The client used to communicate with the API, shared between all modules.
        /// </summary>
        internal static unsafe GoodFriendClient ApiClientInstance { get; private set; } = new GoodFriendClient(new GoodfriendClientOptions()
        {
            BaseAddress = Services.PluginConfiguration.ApiConfig.ApiBaseUrl,
        }, Framework.Instance()->GameVersion.Base);

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelayApiService" /> class.
        /// </summary>
        public unsafe RelayApiService()
        {
            // load higher priority required modules, then required modules, then optional modules in descending order
            var modules = LoadModules().OrderByDescending(x => x.LoadPriority).ThenByDescending(x => x is ApiRequiredModule).ThenByDescending(x => x is ApiOptionalModule).ToList();
            foreach (var module in modules)
            {
                Logger.Information($"Requesting load from module {module.GetType().FullName} with priority {module.LoadPriority}.");

                // If the module is optional, check its configuration
                if (module is ApiOptionalModule optionalModule && optionalModule.Enabled)
                {
                    module.Enable();
                    continue;
                }
                else if (module is not ApiOptionalModule)
                {
                    module.Enable();
                    continue;
                }
            }
            this.loadedModules = modules;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var module in this.loadedModules)
            {
                module.Disable();
            }

            ApiClientInstance.Dispose();
        }

        /// <summary>
        ///     Loads all modules and initializes them.
        /// </summary>
        /// <returns></returns>
        private static ICollection<ApiModuleBase> LoadModules()
        {
            var modules = new List<ApiModuleBase>();
            foreach (var module in typeof(RelayApiService).Assembly.GetTypes())
            {
                try
                {
                    if (module.IsAbstract || !typeof(ApiModuleBase).IsAssignableFrom(module))
                    {
                        continue;
                    }

                    Logger.Information($"Creating instance of module {module.FullName}.");
                    var ctor = module.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException($"The module {module.FullName} does not have a parameterless constructor.");
                    var instance = (ApiModuleBase)ctor.Invoke(null);
                    modules.Add(instance);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to load module {module.FullName}: {e}");
                }
            }
            return modules;
        }

        /// <summary>
        ///     Gets the module service instance.
        /// </summary>
        /// <typeparam name="T">The type of the module to get.</typeparam>
        /// <returns>The module instance if found, otherwise null.</returns>
        public T? GetModule<T>() where T : ApiModuleBase
        {
            foreach (var module in this.loadedModules)
            {
                if (module is T t)
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        ///     Gets all loaded modules.
        /// </summary>
        /// <returns>All loaded modules.</returns>
        public ICollection<ApiModuleBase> GetModules() => this.loadedModules;
    }
}
