# Suggested Commands for Development

## Build and Compilation
```bash
# Build the network plugin
dotnet build networkplugin/NetWorkPlugin.csproj

# Build in release mode
dotnet build networkplugin/NetWorkPlugin.csproj -c Release

# Clean build artifacts
dotnet clean networkplugin/NetWorkPlugin.csproj

# Restore NuGet packages
dotnet restore networkplugin/NetWorkPlugin.csproj
```

## Testing Commands
```bash
# Run the game with the plugin (LBoL.exe must be in same directory)
./LBoL.exe

# Check plugin loading in game console (look for BepInEx logs)
# Check for "Plugin NetworkPlugin is loaded!" message

# Test network functionality
# - Start server instance
# - Connect client instances
# - Verify synchronization events
```

## Git Operations
```bash
# Check git status
git status

# Add changes to staging
git add networkplugin/

# Commit changes
git commit -m "feat: Add network client functionality"

# Push to remote
git push origin dev

# Switch to main branch
git checkout main

# Merge changes from dev
git merge dev
```

## Debugging Commands
```bash
# Check for compilation errors
dotnet build networkplugin/NetWorkPlugin.csproj --verbosity detailed

# Check BepInEx logs in game console or log files
# Look for: "[NetworkPlugin]" prefix in logs

# Verify plugin dependencies
dotnet list package networkplugin/NetWorkPlugin.csproj

# Check assembly dependencies
dotnet list reference networkplugin/NetWorkPlugin.csproj
```

## Development Workflow
```bash
# 1. Make code changes
# 2. Build the plugin
dotnet build networkplugin/NetWorkPlugin.csproj

# 3. Test in LBoL game
# 4. Check logs for errors
# 5. Commit changes with descriptive message
git commit -m "feat: Add auto-reconnection functionality"
```

## System Commands (Windows)
```bash
# Navigate to project directory
cd "c:\Users\oft-Enginner PC\Desktop\LBol_demoplugin"

# List files
dir

# Find files
findstr /s /i "NetworkClient" networkplugin\*.cs

# Check networkplugin directory structure
dir networkplugin\ /s /b
```

## Plugin Development Tips
- Always build after making changes
- Test in LBoL game environment
- Check BepInEx console output for errors
- Use Harmony patches for game modifications
- Register services with dependency injection container
- Implement proper dispose patterns

## Common Development Tasks
```bash
# Add new network event type
# 1. Define event handler in NetworkClient
# 2. Add serialization/deserialization
# 3. Update synchronization logic

# Add new network command
# 1. Create command class
# 2. Add to command processor
# 3. Update network protocol

# Debug connection issues
# 1. Check LiteNetLib logs
# 2. Verify connection key validation
# 3. Test firewall settings
```

## Performance Monitoring
```bash
# Monitor memory usage
# Check game performance during network operations

# Monitor network latency
# Check ping times in connection stats
# Monitor message processing rates
```

Remember to always test thoroughly in the LBoL game environment before committing changes.