# Development Environment Setup

## System Requirements
- **OS**: Windows (as specified in project)
- **.NET SDK**: Compatible with .NET Standard 2.1
- **Game**: LBoL.exe (Little Bloomers Legend)
- **Mod Loader**: BepInEx

## Development Tools
- **IDE**: Visual Studio (recommended) or VS Code with C# extension
- **Build Tool**: .NET CLI (dotnet)
- **Version Control**: Git
- **Modding Framework**: BepInEx with Harmony

## Project Configuration
### Target Framework
```xml
<TargetFramework>netstandard2.1</TargetFramework>
```

### Key Dependencies
- **BepInEx.Core**: Plugin framework for Unity games
- **HarmonyX**: Method patching library
- **LiteNetLib**: Lightweight networking library
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **System.Text.Json**: JSON serialization

### NuGet Sources
```xml
<RestoreAdditionalProjectSources>
  https://api.nuget.org/v3/index.json;
  https://nuget.bepinex.dev/v3/index.json;
  https://nuget.samboy.dev/v3/index.json
</RestoreAdditionalProjectSources>
```

## Directory Structure
```
LBol_demoplugin/
├── networkplugin/           # Main plugin directory
│   ├── Core/               # Core networking logic
│   ├── Network/            # Network components
│   ├── Events/             # Event handling
│   ├── Commands/           # Command system
│   ├── Authority/          # Authority management
│   ├── UI/                 # User interface
│   ├── Utils/              # Utility classes
│   ├── Patch/              # Game patches
│   ├── Chat/               # Chat functionality
│   ├── NetWorkPlugin.csproj  # Project file
│   └── Plugin.cs          # Main plugin entry
├── lbol/                   # Core LBoL game files
│   ├── LBoL.Core/
│   ├── LBoL.Base/
│   ├── LBoL.ConfigData/
│   ├── LBoL.EntityLib/
│   └── LBoL.Presentation/
├── lib/                    # External DLL dependencies
└── .vscode/               # VS Code configuration
```

## Build Configuration
### Debug Build
```bash
dotnet build networkplugin/NetWorkPlugin.csproj -c Debug
```

### Release Build
```bash
dotnet build networkplugin/NetWorkPlugin.csproj -c Release
```

## Game Integration
### BepInEx Plugin Registration
```csharp
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("LBoL.exe")]
public class Plugin : BaseUnityPlugin
```

### Harmony Patching
```csharp
private static readonly Harmony harmony = PluginInfo.harmony;
harmony.PatchAll();
```

## Dependency Injection Setup
```csharp
var services = new ServiceCollection();
services.AddSingleton<INetworkManager, NetworkManager>();
services.AddSingleton<INetworkClient, NetworkClient>();
serviceProvider = services.BuildServiceProvider();
```

## Network Configuration
### Connection Settings
- **Protocol**: TCP via LiteNetLib
- **Serialization**: JSON for complex data, binary for simple data
- **Delivery**: ReliableOrdered for most messages
- **Timeout**: Configurable connection timeout

### Message Types
- **Game Events**: Synchronized game state changes
- **Requests**: Client-to-server queries
- **Responses**: Server-to-client replies
- **System Messages**: Connection status, errors

## Development Workflow
1. **Code Changes**: Modify C# files in networkplugin/
2. **Build**: Compile with `dotnet build`
3. **Test**: Run LBoL.exe and test functionality
4. **Debug**: Check BepInEx console output
5. **Commit**: Commit changes with descriptive message

## Common Issues and Solutions
### Plugin Not Loading
- Check BepInEx installation
- Verify plugin GUID uniqueness
- Check LBoL.exe process name
- Review BepInEx logs for errors

### Network Connection Issues
- Check firewall settings
- Verify server address and port
- Validate connection keys
- Check LiteNetLib logs for errors

### Build Errors
- Ensure .NET SDK compatibility
- Check NuGet package dependencies
- Verify reference assemblies
- Review project configuration

## Performance Considerations
- Use object pooling for network objects
- Minimize garbage collection during network operations
- Use efficient serialization strategies
- Implement proper connection pooling

## Security Considerations
- Validate all incoming network data
- Use secure authentication methods
- Implement proper error handling
- Log security-related events

This environment setup ensures a stable development environment for creating and testing network functionality for the LBoL game.