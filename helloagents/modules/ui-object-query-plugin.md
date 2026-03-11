# ui-object-query-plugin

## 职责

提供一个独立的 BepInEx 调试插件，通过本地 HTTP 请求按 Unity 对象名查询运行时对象属性，重点覆盖 UI 控件的布局信息。

## 接口定义

### HTTP 接口
| 方法 | 路径 | 请求 | 响应 | 说明 |
|------|------|------|------|------|
| `GET` | `/health` | 无 | `{ ok, plugin, version, host, port }` | 健康检查 |
| `POST` | `/query` | `{ name, includeInactive?, maxResults? }` | `{ success, error?, name, count, includeInactive, matches[] }` | 按 `GameObject.name` 精确匹配查询 |

### 响应模型
| 字段 | 类型 | 说明 |
|------|------|------|
| `matches[].path` | string | 从根节点到目标对象的层级路径 |
| `matches[].transform` | object | `position/localPosition/localScale/lossyScale/eulerAngles` 等基础变换信息 |
| `matches[].rectTransform` | object/null | UI 控件专属布局字段，如 `anchoredPosition/sizeDelta/anchor/pivot/offset` |

## 行为规范

### 场景: 查询普通场景对象
**条件**: 插件已加载，外部工具发送 `/query` 请求
**行为**: 插件在主线程扫描 `Resources.FindObjectsOfTypeAll<GameObject>()`，过滤无效 scene 对象后按 `name` 精确匹配
**结果**: 返回所有命中对象的路径、激活状态与 Transform 数据

### 场景: 查询 UI 控件
**条件**: 命中的对象带有 `RectTransform`
**行为**: 在基础 Transform 数据之外追加 `anchoredPosition`、`sizeDelta`、`anchorMin/anchorMax`、`pivot`、`offsetMin/offsetMax`
**结果**: 调用方可直接用于定位 UI 布局问题

### 场景: 后台 HTTP + Unity 主线程协作
**条件**: HTTP 服务收到请求
**行为**: 后台线程只负责收发 JSON，请求实际执行通过插件 `Update()` 中的主线程队列调度
**结果**: 避免跨线程直接访问 Unity API 导致的不稳定行为

## 依赖关系

```yaml
依赖:
  - BepInEx.Core
  - Newtonsoft.Json
  - UnityEngine
  - lbol（仅运行进程和游戏对象上下文）
被依赖: 无
```
