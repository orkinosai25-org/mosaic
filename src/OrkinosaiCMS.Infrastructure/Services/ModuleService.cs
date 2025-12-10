using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Modules.Abstractions;
using ModuleEntity = OrkinosaiCMS.Core.Entities.Sites.Module;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service for managing and discovering modules
/// </summary>
public class ModuleService : IModuleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(ApplicationDbContext context, ILogger<ModuleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ModuleEntity>> GetAllModulesAsync()
    {
        return await _context.Modules
            .Where(m => m.IsEnabled)
            .ToListAsync();
    }

    public async Task<ModuleEntity?> GetModuleByIdAsync(int id)
    {
        return await _context.Modules.FindAsync(id);
    }

    public async Task<ModuleEntity?> GetModuleByNameAsync(string name)
    {
        return await _context.Modules
            .FirstOrDefaultAsync(m => m.Name == name);
    }

    public async Task<ModuleEntity> RegisterModuleAsync(ModuleEntity module)
    {
        var existing = await GetModuleByNameAsync(module.Name);
        if (existing != null)
        {
            // Update existing module
            existing.Title = module.Title;
            existing.Description = module.Description;
            existing.Version = module.Version;
            existing.Category = module.Category;
            existing.AssemblyName = module.AssemblyName;
            existing.ComponentType = module.ComponentType;
            existing.IconCssClass = module.IconCssClass;
            existing.IsEnabled = module.IsEnabled;
            existing.ModifiedOn = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return existing;
        }

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();
        return module;
    }

    public async Task<int> DiscoverAndRegisterModulesAsync()
    {
        var count = 0;
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                foreach (var moduleType in moduleTypes)
                {
                    var attribute = moduleType.GetCustomAttribute<ModuleAttribute>();
                    if (attribute != null)
                    {
                        var module = new ModuleEntity
                        {
                            Name = attribute.Name,
                            Title = attribute.Title,
                            Description = attribute.Description,
                            Category = attribute.Category,
                            Version = attribute.Version,
                            AssemblyName = assembly.FullName ?? assembly.GetName().Name ?? string.Empty,
                            ComponentType = moduleType.FullName ?? moduleType.Name,
                            IconCssClass = attribute.IconCssClass,
                            IsEnabled = true,
                            IsSystem = false,
                            CreatedBy = "System",
                            CreatedOn = DateTime.UtcNow
                        };

                        await RegisterModuleAsync(module);
                        count++;
                        _logger.LogInformation("Registered module: {ModuleName} v{Version}", module.Name, module.Version);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error discovering modules in assembly: {AssemblyName}", assembly.FullName);
            }
        }

        return count;
    }
}
