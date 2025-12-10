using Microsoft.AspNetCore.Components;

namespace OrkinosaiCMS.Modules.Abstractions;

/// <summary>
/// Base class for all Blazor module components
/// Provides common functionality and hooks
/// </summary>
public abstract class ModuleBase : ComponentBase, IModule
{
    /// <summary>
    /// Module settings injected at runtime
    /// </summary>
    [Parameter]
    public IDictionary<string, object>? Settings { get; set; }

    /// <summary>
    /// Unique module identifier
    /// </summary>
    public abstract string ModuleName { get; }

    /// <summary>
    /// Display title for the module
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    /// Module description
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Module version
    /// </summary>
    public virtual string Version => "1.0.0";

    /// <summary>
    /// Module category
    /// </summary>
    public virtual string Category => "General";

    /// <summary>
    /// Initialize the module with settings
    /// </summary>
    public virtual Task InitializeAsync(IDictionary<string, object> settings)
    {
        Settings = settings;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validate module settings
    /// </summary>
    public virtual Task<bool> ValidateSettingsAsync(IDictionary<string, object> settings)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Called when the component is initialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (Settings != null)
        {
            await InitializeAsync(Settings);
        }
        await base.OnInitializedAsync();
    }
}
