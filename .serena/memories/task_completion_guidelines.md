# Task Completion Guidelines

## When a Task is Completed

### Code Quality Checks
1. **Compilation Verification**
   - Build the project to ensure no compilation errors
   - Run: `dotnet build networkplugin/NetWorkPlugin.csproj`

2. **Documentation Review**
   - Ensure all public methods have XML documentation comments
   - Verify inline comments are clear and meaningful
   - Check that code follows established naming conventions

3. **Code Style Consistency**
   - Follow PascalCase for classes and methods
   - Use camelCase for private fields with underscore prefix
   - Maintain consistent indentation (4 spaces)
   - Use Chinese language for comments to match existing codebase

4. **Error Handling**
   - Ensure proper exception handling for network operations
   - Verify meaningful error logging using BepInEx Logger
   - Check that edge cases are handled gracefully

### Network Functionality Testing
1. **Connection Testing**
   - Test client-server connection establishment
   - Verify connection state management
   - Test disconnection handling

2. **Data Transmission Testing**
   - Test game event transmission
   - Verify JSON serialization/deserialization
   - Check request/response handling

3. **Synchronization Testing**
   - Verify game state synchronization
   - Test event propagation
   - Check conflict resolution

### Performance Considerations
1. **Memory Management**
   - Ensure proper disposal of network resources
   - Check for memory leaks in long-running connections
   - Verify efficient data serialization

2. **Network Efficiency**
   - Minimize unnecessary network traffic
   - Use appropriate delivery methods (ReliableOrdered, etc.)
   - Optimize message sizes

### Integration Checklist
1. **BepInEx Integration**
   - Verify plugin loads correctly in LBoL
   - Check Harmony patches apply successfully
   - Ensure dependency injection services are registered

2. **Game Compatibility**
   - Test with different LBoL game scenarios
   - Verify no conflicts with existing game systems
   - Check performance impact on game framerate

### Documentation Updates
1. **Code Documentation**
   - Update method documentation with new parameters
   - Add comments for complex logic
   - Ensure XML documentation is comprehensive

2. **Project Documentation**
   - Update project structure if new files added
   - Add new functionality to overview
   - Update configuration examples if needed

### Final Verification Steps
1. **Build Test**
   ```bash
   dotnet build networkplugin/NetWorkPlugin.csproj
   ```

2. **Plugin Loading Test**
   - Start LBoL with plugin
   - Check for successful plugin loading in console
   - Verify no startup errors

3. **Basic Functionality Test**
   - Test network connection establishment
   - Verify basic message transmission
   - Check error handling for basic scenarios

### Commit Guidelines
1. **Commit Message Format**
   ```
   feat: Add new network functionality
   fix: Resolve connection timeout issue
   docs: Update API documentation
   refactor: Improve network protocol efficiency
   ```

2. **Code Review**
   - Self-review code for quality
   - Check that all todo items are completed
   - Verify follow project conventions

3. **Push to Repository**
   - Commit changes with descriptive message
   - Push to appropriate branch (dev for development, main for stable)

### Post-Commit Tasks
1. **Monitor for Issues**
   - Watch for any reported issues after deployment
   - Be prepared for bug fixes or improvements

2. **Update Roadmap**
   - Mark completed tasks in project roadmap
   - Update feature documentation if needed
   - Plan next development phase

### Special Considerations for Network Plugin
- **Security**: Ensure connection keys are properly validated
- **Reliability**: Implement proper error recovery mechanisms
- **Compatibility**: Test with different network environments
- **Performance**: Monitor network latency and resource usage

Always run a final comprehensive test before marking any task as completed, especially for network-related functionality that affects multiplayer gameplay.