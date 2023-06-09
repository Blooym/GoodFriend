using System;
using System.Collections.Generic;
using System.Linq;
using GoodFriend.Plugin.Base;
using GoodFriend.Plugin.ModuleSystem.Modules;
using GoodFriend.Plugin.ModuleSystem.Modules.Optional;
using GoodFriend.Plugin.ModuleSystem.Modules.Required;

namespace GoodFriend.Plugin.ModuleSystem
{
    internal sealed class ModuleService : IDisposable
    {
        /// <summary>
        ///     All loaded modules.
        /// </summary>
        private readonly ICollection<ModuleBase> loadedModules;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleService" /> class.
        /// </summary>
        public ModuleService()
        {
            // Load required modules first and then by load priority.
            var modules = LoadModules().OrderByDescending(x => x.LoadPriority).ThenByDescending(x => x is RequiredModuleBase).ToList();
            foreach (var module in modules)
            {
                Logger.Information($"Requesting load from module {module.GetType().FullName} with priority {module.LoadPriority}.");

                // If the module is optional, check its configuration
                if (module is OptionalModuleBase optionalModule && optionalModule.Enabled)
                {
                    module.Enable();
                    continue;
                }
                else if (module is not OptionalModuleBase)
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
        }

        /// <summary>
        ///     Loads all modules and initializes them.
        /// </summary>
        /// <returns></returns>
        private static ICollection<ModuleBase> LoadModules()
        {
            var modules = new List<ModuleBase>();
            foreach (var module in typeof(ModuleService).Assembly.GetTypes())
            {
                try
                {
                    if (module.IsAbstract || !typeof(ModuleBase).IsAssignableFrom(module))
                    {
                        continue;
                    }

                    Logger.Information($"Creating instance of module {module.FullName}.");
                    var ctor = module.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException($"The module {module.FullName} does not have a parameterless constructor.");
                    var instance = (ModuleBase)ctor.Invoke(null);
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
        public T? GetModule<T>() where T : ModuleBase
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
        public ICollection<ModuleBase> GetModules() => this.loadedModules;
    }
}
