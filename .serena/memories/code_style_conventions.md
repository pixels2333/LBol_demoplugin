# Code Style and Conventions

## Naming Conventions
### Classes
- Use PascalCase: `NetworkClient`, `SynchronizationManager`, `NetworkPlayer`
- Interface names start with 'I': `INetworkClient`, `INetworkManager`
- Generic type parameters use single letters: `T`, `K`, `V`

### Methods
- Use PascalCase: `ConnectToServer`, `SendGameEvent`, `HandleGameEvent`
- Event handlers use 'EventHandler' suffix: `NetworkEventHandler`
- Async methods use 'Async' suffix when appropriate

### Properties
- Use PascalCase: `IsConnected`, `ConnectionKey`, `ServerPeer`
- Boolean properties use positive form: `CanConnect`, `HasConnection`

### Fields
- Use camelCase with underscore prefix: `_listener`, `_netManager`, `_serverPeer`
- Private fields use underscore prefix
- Static fields use underscore prefix and PascalCase: `_instance`

### Events
- Use PascalCase starting with 'On': `OnConnected`, `OnDisconnected`, `OnGameEventReceived`

## Documentation Standards
### XML Documentation Comments
- All public classes, methods, and properties require XML documentation
- Use `<summary>` for class/method description
- Use `<param>` for method parameters
- Use `<returns>` for return values
- Use `<typeparam>` for generic type parameters

### XML Documentation Format
```csharp
/// <summary>
/// Brief description of the class/method
/// </summary>
/// <param name="parameterName">Description of parameter</param>
/// <returns>Description of return value</returns>
/// <typeparam name="T">Description of generic type parameter</typeparam>
```

### Line Comments
- Use meaningful line comments for complex logic
- Comment should explain 'why' not 'what'
- Use Chinese language for comments to match existing codebase
- Keep comments concise and clear

## File Structure
### Namespace Organization
```
NetworkPlugin.Network.Client      // Client-side components
NetworkPlugin.Network.Server      // Server-side components  
NetworkPlugin.Core               // Core functionality
NetworkPlugin.Events             // Event handling
NetworkPlugin.Commands            // Command processing
NetworkPlugin.Network.NetworkPlayer // Player management
```

## Code Organization
- Use regions to organize related code blocks in larger files
- Keep methods focused and single-purpose
- Use proper indentation (4 spaces)
- Follow existing code patterns and architecture

## Error Handling
- Use try-catch blocks for network operations
- Log errors using BepInEx Logger
- Provide meaningful error messages
- Implement graceful degradation where possible

## Network Communication
- Use JSON serialization for complex data structures
- Use LiteNetLib's NetDataWriter/NetDataReader for efficient binary data
- Implement proper message type identification
- Handle connection state changes appropriately