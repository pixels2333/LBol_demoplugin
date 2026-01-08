# Chat 模块

目录：`Chat/`

## 职责
- 控制台/逻辑层聊天：维护聊天历史、发送/接收聊天消息。
- 与 UI 的关系：UI 层（`UI/Components/ChatUI.cs`）负责展示与输入；Chat 模块提供消息模型与基础能力。

## 关键文件
- `Chat/ChatConsole.cs`：发送/接收消息、历史管理
- `Chat/ChatMessage.cs`：聊天消息 DTO（含 MessageType、Timestamp、PlayerId、Username、Content 等）

## 网络约定
- Chat 通常通过 `NetworkMessageTypes.ChatMessage` 发送（负载为 JSON 序列化的 `ChatMessage`）。

