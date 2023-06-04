namespace GoodFriend.Plugin.Api.ModuleSystem
{
    /// <summary>
    ///     A module that is required and cannot be disabled.
    /// </summary>
    internal abstract class ApiRequiredModule : ApiModuleBase
    {
    }

    /// <summary>
    ///     A module config for a <see cref="ApiRequiredModule"/>s.
    /// </summary>
    internal abstract class ApiRequiredModuleConfig : ApiModuleConfigBase
    {
    }
}