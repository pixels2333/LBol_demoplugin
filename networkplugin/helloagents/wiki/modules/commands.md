# Commands 模块

目录：`Commands/`

## 职责
- 定义可扩展的命令系统抽象（便于控制台/调试/管理命令扩展）。

## 关键文件
- `Commands/ICommand.cs`：命令接口与 `CommandResult`
- `Commands/CommandAttribute.cs`：命令元数据（Name/Usage/Description/RequireAdmin）

## 使用方式（约定）
- 通过 `CommandAttribute` 标注命令信息，并实现 `ICommand.Execute(...)` 与 `GetHelp()`。
- 执行结果使用 `CommandResult` 表达成功/失败、是否广播、提示信息等。

