using ModuleEntity = OrkinosaiCMS.Core.Entities.Sites.Module;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service for managing modules
/// </summary>
public interface IModuleService
{
    /// <summary>
    /// Get all available modules
    /// </summary>
    Task<IEnumerable<ModuleEntity>> GetAllModulesAsync();

    /// <summary>
    /// Get module by ID
    /// </summary>
    Task<ModuleEntity?> GetModuleByIdAsync(int id);

    /// <summary>
    /// Get module by name
    /// </summary>
    Task<ModuleEntity?> GetModuleByNameAsync(string name);

    /// <summary>
    /// Register a new module
    /// </summary>
    Task<ModuleEntity> RegisterModuleAsync(ModuleEntity module);

    /// <summary>
    /// Discover and register modules from assemblies
    /// </summary>
    Task<int> DiscoverAndRegisterModulesAsync();
}
