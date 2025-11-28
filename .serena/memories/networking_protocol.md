# Network Protocol Documentation

## Overview
The network protocol for the LBoL Network Plugin uses LiteNetLib for reliable TCP communication with custom message routing and JSON serialization for complex data structures.

## Protocol Architecture
```
Client <-> LiteNetLib (TCP) <-> Server
     |
     ├── Game Events (JSON)
     ├── Requests/Responses (JSON/Binary)
     └── System Messages (Binary)
```

## Message Types

### 1. Game Events
**Purpose**: Synchronize game state between players
**Format**: JSON
**Direction**: Client ↔ Server

#### Event Structure
```json
{
  "eventType": "string",
  "eventData": "object",
  "timestamp": "long"
}
```

#### Supported Event Types
- `On*` prefix: Game state changes
- `Mana*` prefix: Mana-related events  
- `Gap*` prefix: Gap/station events
- `Battle*` prefix: Battle events
- `StateSyncResponse`: Synchronization responses

### 2. System Requests
**Purpose**: Client-server communication for queries and commands
**Format**: JSON for complex data, binary for primitives
**Direction**: Client → Server

#### Request Structure
```json
{
  "requestHeader": "string",
  "requestData": "object"
}
```

#### Special Response Types
- `*GetSelf_RESPONSE`: Server response identifier

### 3. Connection Messages
**Purpose**: Connection management and authentication
**Format**: Binary
**Direction**: Client ↔ Server

#### Connection Handshake
1. Client sends connection key
2. Server validates authentication
3. Connection established or rejected

## Message Routing

### Client Message Processing
```csharp
// NetworkReceiveEvent handler
string messageType = dataReader.GetString();

if (IsGameEvent(messageType)) {
    HandleGameEvent(messageType, dataReader);
} 
else if (messageType.EndsWith("GetSelf_RESPONSE")) {
    HandleRequestResponse(fromPeer, dataReader);
}
else {
    // Unknown message type
}
```

### Server Message Processing
(To be documented based on server implementation)

## Data Serialization

### JSON Serialization
Used for complex data structures:
- Game event data
- Request/response payloads
- Player information
- Configuration data

### Binary Serialization
Used for primitive types:
- Numeric values (int, float, long, double)
- Boolean values
- String identifiers

## Connection Management

### Connection States
- **Disconnected**: No active connection
- **Connecting**: Connection attempt in progress
- **Connected**: Successfully authenticated and communicating
- **Disconnecting**: Connection termination in progress

### Auto-Reconnection
- **Enabled**: Automatic reconnection attempts on disconnect
- **Retry Interval**: Configurable delay between attempts (default: 5000ms)
- **Timeout**: Configurable connection timeout (default: 5000ms)

## Error Handling

### Network Errors
- **Connection Timeout**: Automatic disconnection after timeout
- **Authentication Failure**: Connection rejected with error message
- **Message Processing Error**: Logged and handled gracefully

### Error Recovery
- Automatic reconnection when enabled
- Graceful degradation for non-critical operations
- Proper resource cleanup on disconnect

## Performance Considerations

### Message Optimization
- Use binary encoding for small messages
- Use JSON encoding for complex structured data
- Implement message size limits
- Use reliable ordered delivery for important messages

### Connection Management
- Pool connection objects
- Implement proper connection lifecycle
- Monitor connection health
- Handle connection spikes gracefully

## Security

### Authentication
- Connection-based authentication using keys
- Per-connection unique identifiers
- Secure key exchange mechanism

### Data Validation
- Input validation for all incoming messages
- Type checking for serialized data
- Bounds checking for array data
- Sanitization of user-provided data

## Protocol Versioning

### Version Compatibility
- Current protocol version: 1.0
- Forward compatibility considerations
- Version negotiation handshake

### Extension Points
- Custom event types
- Additional message formats
- Protocol extensions

This network protocol provides a solid foundation for multiplayer functionality in LBoL, with room for future expansion and optimization based on gameplay requirements.