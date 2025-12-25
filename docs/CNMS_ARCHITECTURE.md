# Core CMS Module (cnms) Architecture

## Overview

The **Core CMS Module (cnms)** is a production-ready CMS framework for OrkinosaiCMS that consolidates proven architectural patterns from **Oqtane** and **Umbraco** open source projects. It provides robust extensibility, master page and layout features, a flexible theme engine, and designer-friendly development patterns.

## Design Philosophy

The cnms module follows these core principles:

1. **Proven Patterns Only**: Uses only tested, production-ready code patterns from Oqtane and Umbraco
2. **Clean Architecture**: Clear separation of concerns with interfaces and implementations
3. **Extensibility First**: Plugin architecture allows unlimited customization without modifying core code
4. **Designer-Friendly**: HTML-based templates, CSS-friendly class naming, declarative configuration
5. **Production-Ready**: Comprehensive error handling, logging, validation, and caching

## Architecture Components

### 1. Theme Engine (Umbraco-Inspired)

The theme engine provides robust theme discovery, loading, and management capabilities.

#### Key Interfaces
- **IThemeProvider**: Theme discovery and management
- **ThemeDescriptor**: Theme metadata and configuration

#### Features
- Theme discovery from database
- Theme validation and asset resolution
- Asset path resolution (`/themes/{themeName}/assets/{assetPath}`)
- Theme caching for performance
- Support for light/dark modes
- Theme metadata (author, version, preview images)

#### Umbraco Patterns Used
- Theme discovery and registration
- Asset resolution and path management
- Theme validation and integrity checking
- Theme metadata and descriptors

### 2. Master Page System (SharePoint/Umbraco-Inspired)

The master page system provides a slot-based content rendering model similar to SharePoint and Umbraco.

#### Key Interfaces
- **IMasterPageRenderer**: Master page rendering with slots
- **ContentSlot**: Named areas for content injection
- **MasterPageRenderContext**: Context for rendering

#### Features
- Slot-based content areas (header, navigation, content, footer, scripts)
- Required vs. optional slots
- Slot content providers (async functions)
- Master page validation
- Predefined master pages (standard, full-width, blog)

#### SharePoint/Umbraco Patterns Used
- Master page inheritance model
- Content placeholders (slots)
- Required/optional slot validation
- Nested master pages support (future)

### 3. Layout Engine (Umbraco Grid/Oqtane-Inspired)

The layout engine provides a flexible, Bootstrap-based grid system for responsive layouts.

#### Key Interfaces
- **ILayoutEngine**: Layout management and rendering
- **LayoutTemplate**: Pre-defined layout structures
- **LayoutConfiguration**: Runtime layout configuration

#### Features
- Bootstrap 12-column grid system
- Layout templates (single-column, two-column, three-column)
- Layout areas and cells
- Module placement in cells
- Layout validation
- Dynamic layout rendering
- Responsive design support

#### Umbraco Grid/Oqtane Patterns Used
- Grid-based layout system
- Layout areas and cells
- Template-based layout creation
- Configuration-driven rendering
- Container concept from Oqtane

### 4. Module Lifecycle Management (Oqtane-Inspired)

The module lifecycle manager handles module initialization, configuration, and disposal following Oqtane's proven patterns.

#### Key Interfaces
- **IModuleLifecycleManager**: Module lifecycle operations
- **ModuleInitResult**: Initialization result
- **ModuleValidationResult**: Configuration validation

#### Features
- Module initialization with state management
- Configuration management
- Validation of module settings
- Module disposal and cleanup
- Dependency tracking
- Module state persistence

#### Oqtane Patterns Used
- Module initialization lifecycle
- Configuration management
- State management
- Dependency resolution
- Clean disposal pattern

### 5. Extensibility Infrastructure (Umbraco-Inspired)

The extensibility infrastructure provides a flexible plugin system for unlimited customization.

#### Key Interfaces
- **IExtensionPointManager**: Extension point management
- **ExtensionPointDescriptor**: Extension point metadata
- **IAsyncExtension**: Base interface for async extensions
- **ISyncExtension**: Base interface for sync extensions

#### Features
- Named extension points
- Type-safe extension registration
- Extension discovery and execution
- Multiple extensions per point
- Extension execution results
- Predefined extension points:
  - `content-rendering`: Modify content before rendering
  - `theme-loading`: Customize theme loading
  - `module-init`: Participate in module initialization
  - `authentication`: Custom authentication providers

#### Umbraco Patterns Used
- Extension point system
- Extension discovery and registration
- Coordinated extension execution
- Extension metadata and descriptors
- Plugin architecture

## Integration with OrkinosaiCMS

The cnms module integrates seamlessly with the existing OrkinosaiCMS infrastructure:

### Service Registration

```csharp
// In Program.cs
using OrkinosaiCMS.Cnms.Extensions;

builder.Services.AddCoreCmsModule(options =>
{
    options.EnableThemeCaching = true;
    options.EnableLayoutValidation = true;
    options.DefaultTheme = "default";
    options.DefaultMasterPage = "standard";
    options.DefaultLayout = "single-column";
});
```

### Dependency Injection

All cnms services are registered with dependency injection:

- **IThemeProvider**: Theme management (Scoped)
- **ILayoutEngine**: Layout rendering (Scoped)
- **IMasterPageRenderer**: Master page rendering (Scoped)
- **IModuleLifecycleManager**: Module lifecycle (Scoped)
- **IExtensionPointManager**: Extension points (Singleton)

### Existing Service Integration

cnms uses existing OrkinosaiCMS services:

- **IThemeService**: Theme data access
- **IModuleService**: Module data access
- **ILogger<T>**: Logging infrastructure

## Usage Patterns

### Theme Management

```csharp
@inject IThemeProvider ThemeProvider

// Get available themes
var themes = await ThemeProvider.GetAvailableThemesAsync();

// Get active theme
var activeTheme = await ThemeProvider.GetActiveThemeAsync(siteId);

// Validate theme
var validation = await ThemeProvider.ValidateThemeAsync("my-theme");
```

### Master Page Rendering

```csharp
@inject IMasterPageRenderer MasterPageRenderer

var context = new MasterPageRenderContext
{
    MasterPageName = "standard",
    PageId = pageId,
    SiteId = siteId
};

context.SlotContent["content"] = async () => "<p>Main content</p>";
var result = await MasterPageRenderer.RenderMasterPageAsync(context);
```

### Layout Engine

```csharp
@inject ILayoutEngine LayoutEngine

var config = await LayoutEngine.CreateFromTemplateAsync("two-column-8-4", settings);
var context = new LayoutRenderContext
{
    LayoutName = "two-column-8-4",
    Configuration = config
};
var result = await LayoutEngine.RenderLayoutAsync(context);
```

### Extensibility

```csharp
@inject IExtensionPointManager ExtensionManager

// Register extension
ExtensionManager.RegisterExtension("content-rendering", myExtension);

// Execute extension point
var result = await ExtensionManager.ExecuteExtensionPointAsync(
    "content-rendering", context);
```

## Predefined Resources

### Master Pages

1. **standard**: Full-featured master page
   - Slots: head, header, navigation, content, footer, scripts
   - Use case: Standard pages with full chrome

2. **full-width**: Minimal master page
   - Slots: head, content, scripts
   - Use case: Landing pages, full-width content

3. **blog**: Blog-specific master page
   - Slots: head, header, navigation, content, sidebar, footer, scripts
   - Use case: Blog posts with sidebar

### Layout Templates

1. **single-column**: Simple single column (12-span)
2. **two-column-8-4**: Two columns with 8-4 split
3. **three-column-3-6-3**: Three columns with 3-6-3 split

### Extension Points

1. **content-rendering**: Modify content before rendering
2. **theme-loading**: Customize theme loading behavior
3. **module-init**: Participate in module initialization
4. **authentication**: Custom authentication providers

## Performance Considerations

### Caching

- Theme descriptors are cached in memory
- Cache invalidation on theme updates
- Configurable caching via CnmsOptions

### Validation

- Theme validation before activation
- Layout validation before rendering
- Module configuration validation
- Configurable validation via CnmsOptions

### Logging

- Comprehensive logging at all levels
- Structured logging with context
- Error tracking and diagnostics
- Performance monitoring

## Benefits

### For Developers

1. **Clean Architecture**: Interfaces separate from implementations
2. **Testability**: Dependency injection throughout
3. **Extensibility**: Plugin architecture without core modifications
4. **Documentation**: Comprehensive XML comments

### For Designers

1. **HTML Templates**: Familiar master page structure
2. **CSS-Friendly**: Clear class naming conventions
3. **Bootstrap Grid**: 12-column responsive system
4. **Declarative**: Configuration-driven layouts

### For Administrators

1. **Theme Management**: Easy theme switching
2. **Layout Control**: Visual layout selection
3. **Module Management**: Lifecycle control
4. **Validation**: Built-in integrity checks

## Future Enhancements

### Phase 1 (Completed)
- ✅ Theme engine
- ✅ Master page system
- ✅ Layout engine
- ✅ Module lifecycle
- ✅ Extensibility infrastructure

### Phase 2 (Planned)
- [ ] Theme packaging and distribution
- [ ] Visual layout designer
- [ ] Extension marketplace
- [ ] Advanced caching strategies
- [ ] Performance monitoring
- [ ] A/B testing support

### Phase 3 (Future)
- [ ] Multi-level master page inheritance
- [ ] Dynamic theme switching
- [ ] Theme preview mode
- [ ] Layout analytics
- [ ] Advanced module dependencies

## Comparison with Source Projects

### From Oqtane

| Feature | Oqtane | cnms Implementation |
|---------|--------|-------------------|
| Module Lifecycle | ✓ | ✓ Adapted |
| Container System | ✓ | ✓ As Layout Engine |
| Extension Points | ✓ | ✓ Enhanced |
| Authentication | ✓ | ✓ Extension Point |

### From Umbraco

| Feature | Umbraco | cnms Implementation |
|---------|---------|-------------------|
| Theme System | ✓ | ✓ Adapted |
| Grid Layout | ✓ | ✓ As Layout Engine |
| Extension System | ✓ | ✓ Adapted |
| Master Pages | ✓ | ✓ Slot-based |

### From SharePoint

| Feature | SharePoint | cnms Implementation |
|---------|-----------|-------------------|
| Master Pages | ✓ | ✓ Content Slots |
| Web Parts | ✓ | ✓ Module System |
| Layouts | ✓ | ✓ Layout Engine |

## Conclusion

The cnms module represents a production-ready CMS framework that consolidates the best practices from Oqtane, Umbraco, and SharePoint. It provides:

- **Robust Architecture**: Clean, testable, extensible
- **Proven Patterns**: Battle-tested in production systems
- **Designer-Friendly**: HTML templates, Bootstrap grid
- **Production-Ready**: Error handling, validation, logging
- **Future-Proof**: Extensibility infrastructure for growth

This architecture ensures maintainability, scalability, and long-term success of the OrkinosaiCMS platform.
