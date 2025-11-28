# LBoL Network Plugin Project Overview

## Project Purpose
This is a network plugin for Little Bloomers Legend (LBoL), a Touhou Project-inspired card battle game. The plugin adds online multiplayer functionality, allowing players to connect and play together over a network connection.

## Tech Stack
- **Language**: C#
- **Framework**: .NET Standard 2.1
- **Game Platform**: Unity-based (LBoL.exe)
- **Modding Framework**: BepInEx (plugin loader for Unity games)
- **Networking**: LiteNetLib (lightweight networking library)
- **Harmony**: Method patching library for game modding
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **JSON Serialization**: System.Text.Json

## Project Structure
```
networkplugin/
├── Core/                    # Core networking logic
│   └── SynchronizationManager.cs
├── Network/                 # Network components
│   ├── Client/
│   │   └── NetworkClient.cs
│   └── NetworkPlayer/
│       └── INetworkPlayer.cs
├── Events/                  # Event handling
├── Commands/                # Command system
├── Authority/              # Authority management
├── UI/                     # User interface components
├── Utils/                  # Utility classes
├── Patch/                  # Game patches
├── Chat/                   # Chat functionality
└── Plugin.cs               # Main plugin entry point
```

## Key Features
- **Network Client**: Handles connection to game servers
- **Synchronization**: Manages game state synchronization across network
- **Event System**: Custom event handling for network events
- **Auto-reconnection**: Automatic reconnection on disconnect
- **JSON Serialization**: Support for complex data structures
- **Player Management**: Network player management system

## Main Components
- **NetworkClient**: Core client functionality for server communication
- **SynchronizationManager**: Handles game state synchronization
- **INetworkManager**: Interface for network operations
- **INetworkPlayer**: Network player representation