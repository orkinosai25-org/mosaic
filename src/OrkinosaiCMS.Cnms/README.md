# OrkinosaiCMS Core CMS Module (cnms)

## Overview

The **Core CMS Module (cnms)** is a comprehensive, production-ready CMS framework that consolidates proven patterns from **Oqtane** and **Umbraco** open source projects. It provides robust extensibility, master page and layout features, theme engine, and designer-friendly development patterns.

## Architecture

The cnms module follows clean architecture principles and includes:

### 1. Theme Engine (Umbraco-inspired)
- **Theme Provider**: Discovers, loads, and manages themes
- **Theme Descriptors**: Metadata for themes including master pages and layouts
- **Theme Validation**: Ensures theme integrity and structure
- **Asset Resolution**: Resolves theme-specific CSS, JS, and image paths

### 2. Master Page System (SharePoint/Umbraco-inspired)
- **Master Page Renderer**: Renders master pages with content slots
- **Content Slots**: Named areas for content injection (header, navigation, content, footer, etc.)
- **Slot Management**: Register and manage content providers for slots
- **Validation**: Ensures required slots are provided

### 3. Layout Engine (Umbraco Grid/Oqtane-inspired)
- **Layout Templates**: Pre-defined layout structures (single-column, two-column, three-column)
- **Bootstrap Grid**: 12-column grid system for responsive layouts
- **Layout Areas**: Logical grouping of content cells
- **Layout Cells**: Individual content containers with module support
- **Dynamic Rendering**: Runtime layout generation based on configuration

### 4. Module Lifecycle Management (Oqtane-inspired)
- **Initialization**: Module instance setup and state management
- **Configuration**: Module settings and parameters
- **Validation**: Configuration validation and dependency checking
- **Disposal**: Proper cleanup of module resources
- **Dependency Resolution**: Module dependency tracking

### 5. Extensibility Infrastructure (Umbraco-inspired)
- **Extension Points**: Named extension points for pluggable functionality
- **Extension Registration**: Type-safe extension registration
- **Extension Execution**: Coordinated execution of multiple extensions
- **Extension Discovery**: Automatic discovery of available extension points

## Features

### ✅ Proven Patterns
- Uses only tested, production-ready code from Oqtane and Umbraco
- Follows industry best practices for CMS architecture
- Implements clean architecture and SOLID principles

### ✅ Robust Extensibility
- Extension point system for unlimited customization
- Module lifecycle management for clean initialization/disposal
- Type-safe extension interfaces

### ✅ Theme Engine
- Theme discovery and validation
- Asset path resolution
- Light/dark mode support
- Theme metadata and preview images

### ✅ Master Pages & Layouts
- SharePoint-style master pages with content slots
- Umbraco Grid-inspired layout system
- Bootstrap-based responsive grid (12-column)
- Pre-defined layout templates

### ✅ Designer-Friendly
- Declarative layout configuration
- HTML-based master page templates
- CSS-friendly class naming
- Clear separation of structure and presentation

### ✅ Production-Ready
- Comprehensive error handling and logging
- Validation at every layer
- Caching support for performance
- Dependency injection throughout

## Installation

Add the cnms module to your project:

```bash
dotnet add reference ../OrkinosaiCMS.Cnms/OrkinosaiCMS.Cnms.csproj
```

Register services in `Program.cs`:

```csharp
using OrkinosaiCMS.Cnms.Extensions;

// Add Core CMS Module services
builder.Services.AddCoreCmsModule();

// Or with custom configuration
builder.Services.AddCoreCmsModule(options =>
{
    options.DefaultTheme = "my-theme";
    options.DefaultMasterPage = "standard";
    options.DefaultLayout = "two-column-8-4";
    options.EnableThemeCaching = true;
});
```

## Usage Examples

### Theme Management

```csharp
// Inject theme provider
@inject IThemeProvider ThemeProvider

// Get available themes
var themes = await ThemeProvider.GetAvailableThemesAsync();

// Get active theme for a site
var activeTheme = await ThemeProvider.GetActiveThemeAsync(siteId);

// Set active theme
await ThemeProvider.SetActiveThemeAsync(siteId, "my-theme");

// Validate theme
var validation = await ThemeProvider.ValidateThemeAsync("my-theme");
if (!validation.IsValid)
{
    // Handle errors
}

// Resolve asset paths
var cssPath = ThemeProvider.ResolveAssetPath("my-theme", "styles/main.css");
// Returns: /themes/my-theme/assets/styles/main.css
```

### Master Page Rendering

```csharp
// Inject master page renderer
@inject IMasterPageRenderer MasterPageRenderer

// Create render context
var context = new MasterPageRenderContext
{
    MasterPageName = "standard",
    PageId = pageId,
    SiteId = siteId
};

// Register slot content
context.SlotContent["header"] = async () => "<h1>Welcome</h1>";
context.SlotContent["content"] = async () => "<p>Main content here</p>";
context.SlotContent["footer"] = async () => "<footer>© 2024</footer>";

// Render master page
var result = await MasterPageRenderer.RenderMasterPageAsync(context);
if (result.Success)
{
    // Use result.Html
}
```

### Layout Engine

```csharp
// Inject layout engine
@inject ILayoutEngine LayoutEngine

// Get available layouts
var layouts = await LayoutEngine.GetAvailableLayoutsAsync();

// Create layout from template
var settings = new Dictionary<string, object>
{
    { "containerClass", "container-fluid" }
};
var config = await LayoutEngine.CreateFromTemplateAsync("two-column-8-4", settings);

// Render layout
var context = new LayoutRenderContext
{
    LayoutName = "two-column-8-4",
    PageId = pageId,
    Configuration = config
};
var result = await LayoutEngine.RenderLayoutAsync(context);
```

### Module Lifecycle

```csharp
// Inject module lifecycle manager
@inject IModuleLifecycleManager ModuleLifecycle

// Initialize module
var initResult = await ModuleLifecycle.InitializeAsync(moduleInstanceId);
if (initResult.Success)
{
    // Module initialized, access initial state
    var state = initResult.InitialState;
}

// Configure module
var settings = new Dictionary<string, object>
{
    { "title", "My Module" },
    { "displayMode", "grid" }
};
await ModuleLifecycle.ConfigureAsync(moduleInstanceId, settings);

// Validate configuration
var validation = await ModuleLifecycle.ValidateConfigurationAsync(moduleInstanceId);

// Dispose module
await ModuleLifecycle.DisposeAsync(moduleInstanceId);
```

### Extensibility

```csharp
// Inject extension point manager
@inject IExtensionPointManager ExtensionManager

// Register an extension
var myExtension = new MyContentRenderingExtension();
ExtensionManager.RegisterExtension("content-rendering", myExtension);

// Get all extensions for a point
var extensions = ExtensionManager.GetExtensions<IContentRenderingExtension>("content-rendering");

// Execute extension point
var context = new { Content = "Hello", PageId = 1 };
var result = await ExtensionManager.ExecuteExtensionPointAsync("content-rendering", context);
if (result.Success)
{
    // Process results
}
```

## Predefined Resources

### Master Pages
- **standard**: Full-featured master page with header, nav, content, footer, and script slots
- **full-width**: Minimal master page for full-width content
- **blog**: Blog-specific master page with sidebar support

### Layout Templates
- **single-column**: Simple single column layout (12-span)
- **two-column-8-4**: Two column layout with 8-4 split
- **three-column-3-6-3**: Three column layout with 3-6-3 split

### Extension Points
- **content-rendering**: Modify content before rendering
- **theme-loading**: Customize theme loading behavior
- **module-init**: Participate in module initialization
- **authentication**: Custom authentication providers

## Integration with Existing Code

The cnms module integrates seamlessly with existing OrkinosaiCMS infrastructure:

- Uses existing `IThemeRepository` and `IModuleService` interfaces
- Compatible with existing `Theme` and `Module` entities
- Follows established logging patterns with `ILogger`
- Works with existing dependency injection setup

## Benefits

1. **Maintainability**: Clear separation of concerns, well-documented code
2. **Extensibility**: Plugin architecture allows unlimited customization
3. **Performance**: Caching, lazy loading, and efficient rendering
4. **Testability**: Interfaces for all services, dependency injection throughout
5. **Designer-Friendly**: HTML templates, CSS classes, Bootstrap grid
6. **Production-Ready**: Error handling, validation, logging at every layer

## Architecture Patterns

### From Oqtane
- Module lifecycle management
- Extension point system
- Dependency injection patterns
- Authentication integration

### From Umbraco
- Theme system architecture
- Layout grid concept
- Extension architecture
- Master page patterns

### From SharePoint
- Master page and content slot model
- Web part (module) integration
- Page layout concepts

## Future Enhancements

- [ ] Theme packaging and distribution
- [ ] Visual layout designer
- [ ] Extension marketplace integration
- [ ] Advanced caching strategies
- [ ] Performance monitoring
- [ ] A/B testing support

## Contributing

This module follows clean architecture principles. When contributing:

1. Keep interfaces in `Interfaces/` folder
2. Keep implementations in `Services/` folder
3. Follow existing naming conventions
4. Add XML documentation comments
5. Include error handling and logging
6. Write unit tests for new features

## License

This module is part of OrkinosaiCMS and follows the same Apache 2.0 license with usage restrictions.

## Support

For questions or issues:
- 📧 Email: support@orkinosai.com
- 🐛 Issues: GitHub Issues
- 📖 Documentation: See `/docs` folder
