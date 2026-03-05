# 工作区目录说明

> 目的：快速说明当前 VS Code 多根工作区中，各主要目录的职责与用途。
> 范围：`LBol_demoplugin` 与 `Together in Spire v6.4.20`。

## 1. 工作区根目录（多根）

### `LBol_demoplugin/`
以 C#/.NET 为主的联机插件开发主工程，包含插件源码、游戏反编译代码、依赖库与调试工具。

### `Together in Spire v6.4.20/`
以 Java 为主的参考工程（Slay the Spire 联机相关模组代码与资源），用于对照联机架构与功能实现。

---

## 2. `LBol_demoplugin/` 目录说明

### 工程与配置
- `.git/`：Git 版本控制数据。
- `.github/`：仓库级自动化与协作配置（例如工作流、模板）。
- `.vscode/`：VS Code 工作区/调试/任务相关配置。
- `simplesolution.sln`：Visual Studio 解决方案入口，聚合 `debugtools`、`networkplugin` 与 `lbol` 下的多个项目。

### 业务代码与参考代码
- `networkplugin/`：联机 MOD 的核心代码目录（当前主开发区）。
  - `Chat/`：聊天消息与聊天控制台。
  - `Configuration/`：联机、同步、性能等配置管理。
  - `Core/`：同步管理核心接口与实现。
  - `Network/`：网络通信主逻辑（客户端/服务端/房间/消息/重连/中途加入/快照等）。
  - `Patch/`：对游戏行为打补丁的同步逻辑（战斗、地图、网络、UI 等）。
  - `UI/`：联机功能界面组件、对话框、面板、控件。
  - `Utils/`：卡牌、法力、状态、日志、事件缓冲等辅助工具。

- `MyFirstPlugin/`：示例/实验插件工程。
  - `Loader/`：资源或 Spine 加载逻辑。
  - `Patch/`：示例补丁实现。
  - `Resource/`：示例资源（角色等）。

- `lbol/`：LBoL 游戏代码镜像（分层项目），主要用于查类型与对接点。
  - `LBoL.Base/`：基础类型、枚举、扩展与通用结构。
  - `LBoL.ConfigData/`：配置数据模型（卡牌、敌人、关卡、音效等配置）。
  - `LBoL.Core/`：核心玩法逻辑（战斗、回合、事件、地图、存档、状态等）。
  - `LBoL.EntityLib/`：实体与内容库（卡牌、敌人、宝物、关卡等具体实现）。
  - `LBoL.Presentation/`：表现层（UI、动画、音频、场景/视觉相关）。

- `lib/`：外部依赖 DLL（Unity/LBoL/第三方库），供插件编译与运行时引用。

### 工具、文档与计划
- `debugtools/`：独立调试工具项目（文件存在性检查、运行状态验证等）。
- `plan/`：独立方案包目录（按时间戳命名），用于跟踪某次功能计划与实施。
- `helloagents/`：知识库与流程文档目录（索引、上下文、变更记录、模块文档、方案归档等）。
- `lbol-complete-project-doc.md`、`lbol-methods-doc.md`、`lbol-noncore-methods-doc.md`：LBoL 结构/方法参考文档。
- `chat.json`：聊天/上下文配置文件。
- `createproject.ps1`、`copy_networkplugin_dll.ps1`：工程初始化与构建产物复制脚本。

### 常见构建产物目录
- `bin/`、`obj/`（分布在各项目内）：编译输出与中间产物目录。

---

## 3. `Together in Spire v6.4.20/` 目录说明

### 核心代码
- `spireTogether/`：联机主模组核心代码（网络、战斗、地图、事件、UI、存档、补丁等）。
- `dLib/`：通用库层（命令、补丁、UI 元素、工具类、兼容逻辑等基础能力）。
- `skindex/`：皮肤/外观相关子系统（实体皮肤、注册、保存、解锁、兼容适配等）。

### 资源与元数据
- `spireTogetherResources/`：联机模组资源（图片、本地化、着色器）。
- `dLibResources/`：dLib 资源（主要是 UI 图片）。
- `skindexResources/`：skindex 资源（音频、图片、本地化）。
- `META-INF/`：JAR 元信息与 Maven 元数据（`MANIFEST.MF` 等）。
- `ModTheSpire.json`：模组加载入口配置。
- `IDCheckStringsDONT-EDIT-AT-ALL.json`：特定运行校验字符串配置文件（不应手改）。

---

## 4. 快速定位建议

- 想改联机行为：优先看 `networkplugin/Network/` + `networkplugin/Patch/`。
- 想改联机界面：优先看 `networkplugin/UI/`。
- 想确认游戏底层类型/事件：优先看 `lbol/LBoL.Core/` 与 `lbol/LBoL.Base/`。
- 想参考成熟联机实现：优先看 `Together in Spire v6.4.20/spireTogether/` 与 `Together in Spire v6.4.20/dLib/`。
