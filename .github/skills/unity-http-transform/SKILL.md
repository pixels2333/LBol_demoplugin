---
name: unity-http-transform
description: '发送 HTTP 请求到 RuntimeUnityEditor 的 HTTP API，获取 Unity 场景中 GameObject 的 Transform 和 RectTransform 位置、大小、偏移等信息。用于外部工具与 Unity 运行时调试集成。'
user-invocable: true
---

# Unity HTTP Transform API Client

## 使用场景
- 外部工具需要实时获取 Unity 场景中 GameObject 的 Transform 信息
- 自动化测试需要读取 UI 控件（RectTransform）的位置和大小
- 不通过 GUI 界面，而是用 HTTP 请求批量查询物体状态
- 需要查询未激活（隐藏）物体的 Transform 数据

## 前提条件
RuntimeUnityEditor 必须已加载 `HttpTransformApi` 特性，HTTP 服务在指定端口运行（默认 `9876`）。

## 请求格式

```
GET http://localhost:{port}/api/transform?name={GameObjectName}
```

| 参数 | 说明 | 默认值 |
|---|---|---|
| `port` | HTTP API 端口 | `9876` |
| `name` | GameObject 名称 | 必填 |

## 响应格式

### 成功（普通 3D 物体）
```json
{
  "name": "Cube",
  "active": true,
  "transform": {
    "position": {"x":1.000000,"y":2.000000,"z":3.000000},
    "localPosition": {"x":0.000000,"y":0.000000,"z":0.000000},
    "localScale": {"x":1.000000,"y":1.000000,"z":1.000000},
    "eulerAngles": {"x":0.000000,"y":45.000000,"z":0.000000},
    "localEulerAngles": {"x":0.000000,"y":0.000000,"z":0.000000}
  }
}
```

### 成功（UI 控件，含 RectTransform）
```json
{
  "name": "Button",
  "active": true,
  "transform": {
    "position": {"x":0.000000,"y":0.000000,"z":0.000000},
    "localPosition": {"x":0.000000,"y":0.000000,"z":0.000000},
    "localScale": {"x":1.000000,"y":1.000000,"z":1.000000},
    "eulerAngles": {"x":0.000000,"y":0.000000,"z":0.000000},
    "localEulerAngles": {"x":0.000000,"y":0.000000,"z":0.000000}
  },
  "rectTransform": {
    "anchorMin": {"x":0.500000,"y":0.500000},
    "anchorMax": {"x":0.500000,"y":0.500000},
    "offsetMin": {"x":-50.000000,"y":-15.000000},
    "offsetMax": {"x":50.000000,"y":15.000000},
    "sizeDelta": {"x":100.000000,"y":30.000000},
    "pivot": {"x":0.500000,"y":0.500000},
    "anchoredPosition": {"x":0.000000,"y":0.000000}
  }
}
```

### 失败
```json
{"error":"GameObject not found"}
```

## 客户端调用示例

### Python 脚本
使用自带的 `query_transform.py`：

```bash
python scripts/query_transform.py Canvas/Button 9876
```

或直接使用 `curl`：

```bash
curl "http://localhost:9876/api/transform?name=Canvas/Button"
```

### C# HttpClient
```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;

async Task QueryTransform(string name, int port = 9876)
{
    using var client = new HttpClient();
    var response = await client.GetAsync($"http://localhost:{port}/api/transform?name={Uri.EscapeDataString(name)}");
    var json = await response.Content.ReadAsStringAsync();
    Console.WriteLine(json);
}
```

## 脚本资产
- [scripts/query_transform.py](./scripts/query_transform.py) — 基于 urllib 的轻量级查询脚本，无需第三方依赖

## 注意事项
- 端口可在 RuntimeUnityEditor 配置文件中修改（通过 `InitSettings.RegisterSetting` 注册）
- 如需批量查询，可循环调用该接口
- 查找支持隐藏物体（通过 `FindObjectsOfType<Transform>` 回退）
