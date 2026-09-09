---
name: unity-http-transform
description: '发送 HTTP 请求到 RuntimeUnityEditor 的 HTTP API，获取或实时修改 Unity 场景中 GameObject 的 Transform 和 RectTransform 位置、大小、偏移、锚点及子级层级树信息。用于外部工具与 Unity 运行时调试集成。'
---

# Unity HTTP Transform API Client

## 适用场景
- 外部工具或 AI Agent 需要实时获取 Unity 场景中 GameObject / UI 控件的空间信息
- 自动化脚本、测试框架需要动态读取并修改 UI 控件（RectTransform）的位置、尺寸、锚点及激活状态
- 不通过游戏内 GUI 手动拖拽，而是通过 HTTP 接口批量或精准修改物体 Transform 属性
- 快速查询并调整深层嵌套的子级 UI 布局与 3D 实体位置

## 前提条件
RuntimeUnityEditor 必须已加载 `HttpTransformApi` 特性，HTTP 服务在指定端口运行（默认 `9876`，可在配置文件中修改）。

---

## 接口契约与操作详解

### 1. GET — 查询 Transform/RectTransform 及子级层级信息

#### 请求格式
```http
GET http://localhost:{port}/api/transform?name={GameObjectName}&recursive={bool}&depth={int}&includeInactive={bool}
```

#### Query 参数列表
| 参数名 | 类型 | 说明 | 默认值 | 样例 |
|---|---|---|---|---|
| `port` | int | HTTP API 服务端口 | `9876` | `9876` |
| `name` | string | GameObject 名称或层级路径（支持 `/` 分隔） | **必填** | `Canvas` 或 `Canvas/Panel/Button` |
| `recursive` | bool | 是否递归查询子级层级树 | `false` | `true` |
| `depth` | int | 递归深度限制（`1` 为仅直接子级，`-1` 为无限递归） | `1`（当 `recursive=true` 时） | `2` 或 `-1` |
| `includeInactive` | bool | 是否包含隐藏/未激活的子对象 | `false` | `true` |

---

### 2. POST — 修改 Transform/RectTransform 属性与状态

#### 请求格式
```http
POST http://localhost:{port}/api/transform/set?name={GameObjectName}
Content-Type: application/json 或 application/x-www-form-urlencoded
```

#### Query 参数
| 参数名 | 类型 | 说明 |
|---|---|---|
| `name` | string | 要修改的目标 GameObject 名称或层级路径（**必填**，例如 `Button` 或 `Canvas/MainPanel/StartBtn`） |

#### 支持修改的属性列表（增量分量覆盖）
> [!TIP]
> **增量分量覆盖机制**：只传需要修改的字段或分量，未指定的分量会自动保留当前运行时的实际值。

| 字段类别 | 属性名 | 支持分量 | 数据类型 | 说明 |
|---|---|---|---|---|
| **基础状态** | `active` / `activeSelf` | - | `bool` (`true`/`false`) | 激活或隐藏 GameObject |
| **3D Transform** | `position` | `.x`, `.y`, `.z` | `float` | 世界坐标位置 |
| | `localPosition` | `.x`, `.y`, `.z` | `float` | 局部坐标位置 |
| | `localScale` | `.x`, `.y`, `.z` | `float` | 局部缩放倍率 |
| | `eulerAngles` | `.x`, `.y`, `.z` | `float` | 世界旋转欧拉角（度数） |
| | `localEulerAngles` | `.x`, `.y`, `.z` | `float` | 局部旋转欧拉角（度数） |
| **2D RectTransform** | `anchoredPosition` | `.x`, `.y` | `float` | 相对锚点中心的锚定坐标（UI 位移） |
| | `sizeDelta` | `.x`, `.y` | `float` | UI 控件的宽度（.x）与高度（.y）增量 |
| | `anchorMin` | `.x`, `.y` | `float` | 锚点最小值（0.0 ~ 1.0） |
| | `anchorMax` | `.x`, `.y` | `float` | 锚点最大值（0.0 ~ 1.0） |
| | `offsetMin` | `.x`, `.y` | `float` | 左下角偏移边距 |
| | `offsetMax` | `.x`, `.y` | `float` | 右上角偏移边距 |
| | `pivot` | `.x`, `.y` | `float` | 轴心点中心（0.0 ~ 1.0，居中为 0.5, 0.5） |

---

## 请求体格式说明

接口支持 **JSON** 与 **Form 表单** 两种数据格式：

### 格式 A：JSON 结构体 (`application/json`)
支持嵌套对象或扁平键名：
```json
{
  "active": true,
  "transform": {
    "localPosition": { "x": 100.0, "y": 50.0 }
  },
  "rectTransform": {
    "sizeDelta": { "x": 300.0, "y": 80.0 },
    "anchoredPosition": { "x": 0.0, "y": -120.0 }
  }
}
```

### 格式 B：Form 表单 (`application/x-www-form-urlencoded`)
简洁键值对，无需构造 JSON：
```
transform.localPosition.x=100&rectTransform.sizeDelta.x=300&rectTransform.sizeDelta.y=80&active=true
```
*(注：前缀 `transform.` 和 `rectTransform.` 可省略，例如直接传 `sizeDelta.x=300&sizeDelta.y=80` 同样生效)*

---

## 响应数据格式

### 1. 修改成功（返回更新后的最新完整数据）
```json
{
  "name": "StartButton",
  "active": true,
  "transform": {
    "position": {"x": 0.000000, "y": 0.000000, "z": 0.000000},
    "localPosition": {"x": 100.000000, "y": 50.000000, "z": 0.000000},
    "localScale": {"x": 1.000000, "y": 1.000000, "z": 1.000000},
    "eulerAngles": {"x": 0.000000, "y": 0.000000, "z": 0.000000},
    "localEulerAngles": {"x": 0.000000, "y": 0.000000, "z": 0.000000}
  },
  "rectTransform": {
    "anchorMin": {"x": 0.500000, "y": 0.500000},
    "anchorMax": {"x": 0.500000, "y": 0.500000},
    "offsetMin": {"x": -150.000000, "y": -40.000000},
    "offsetMax": {"x": 150.000000, "y": 40.000000},
    "sizeDelta": {"x": 300.000000, "y": 80.000000},
    "pivot": {"x": 0.500000, "y": 0.500000},
    "anchoredPosition": {"x": 0.000000, "y": -120.000000}
  }
}
```

### 2. 查询子级树成功（带有 `recursive=true`）
```json
{
  "name": "Canvas",
  "active": true,
  "transform": { ... },
  "rectTransform": { ... },
  "children": [
    {
      "name": "Panel",
      "active": true,
      "transform": { ... },
      "rectTransform": { ... },
      "children": [ ... ]
    }
  ]
}
```

### 3. 错误响应
```json
{
  "error": "GameObject not found"
}
```

---

## 常用操作场景详解与调用示例

### 场景 1：调整 UI 控件尺寸与坐标
修改 `Canvas/MainPanel/StartButton` 的宽度为 300、高度为 80，并将 Y 轴坐标下移 50：

**Python 脚本：**
```bash
python scripts/query_transform.py --set Canvas/MainPanel/StartButton 9876 rectTransform.sizeDelta.x=300 rectTransform.sizeDelta.y=80 rectTransform.anchoredPosition.y=-50
```

**curl (Form)：**
```bash
curl -X POST "http://localhost:9876/api/transform/set?name=Canvas/MainPanel/StartButton" \
  -d "sizeDelta.x=300&sizeDelta.y=80&anchoredPosition.y=-50"
```

**curl (JSON)：**
```bash
curl -X POST "http://localhost:9876/api/transform/set?name=Canvas/MainPanel/StartButton" \
  -H "Content-Type: application/json" \
  -d '{"rectTransform":{"sizeDelta":{"x":300,"y":80},"anchoredPosition":{"y":-50}}}'
```

---

### 场景 2：调整 UI 控件锚点（如置顶居中）
将控件的锚点设置为顶部居中（`anchorMin=(0.5, 1.0)`, `anchorMax=(0.5, 1.0)`, `pivot=(0.5, 1.0)`）：

**Python 脚本：**
```bash
python scripts/query_transform.py --set HeaderBar 9876 \
  anchorMin.x=0.5 anchorMin.y=1.0 \
  anchorMax.x=0.5 anchorMax.y=1.0 \
  pivot.x=0.5 pivot.y=1.0 \
  anchoredPosition.x=0 anchoredPosition.y=-20
```

---

### 场景 3：隐藏或显示指定物体
**Python 脚本：**
```bash
# 隐藏物体
python scripts/query_transform.py --set Canvas/Panel/PopupDialog 9876 active=false

# 显示物体
python scripts/query_transform.py --set Canvas/Panel/PopupDialog 9876 active=true
```

---

### 场景 4：查询 UI 树结构并递归导出
**Python 脚本：**
```bash
# 查询 Canvas 及其直接子级
python scripts/query_transform.py Canvas 9876 -r

# 查询 Canvas 及其下两层子节点，包含隐藏对象
python scripts/query_transform.py Canvas 9876 -r -d 2 -i

# 全量递归查询整棵树
python scripts/query_transform.py Canvas 9876 -r -d -1
```

---

### 场景 5：C# HttpClient 集成调用

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class UnityTransformClient
{
    private readonly HttpClient _client = new HttpClient();
    private readonly string _baseUrl;

    public UnityTransformClient(string baseUrl = "http://localhost:9876")
    {
        _baseUrl = baseUrl;
    }

    // 1. 查询 Transform / 子级树
    public async Task<string> QueryTransformAsync(string name, bool recursive = false, int depth = 1, bool includeInactive = false)
    {
        var url = $"{_baseUrl}/api/transform?name={Uri.EscapeDataString(name)}&recursive={recursive}&depth={depth}&includeInactive={includeInactive}";
        return await _client.GetStringAsync(url);
    }

    // 2. 修改 Transform / RectTransform (JSON 格式)
    public async Task<string> SetTransformJsonAsync(string name, string jsonPayload)
    {
        var url = $"{_baseUrl}/api/transform/set?name={Uri.EscapeDataString(name)}";
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }

    // 3. 修改 Transform / RectTransform (Form 格式)
    public async Task<string> SetTransformFormAsync(string name, Dictionary<string, string> fields)
    {
        var url = $"{_baseUrl}/api/transform/set?name={Uri.EscapeDataString(name)}";
        var content = new FormUrlEncodedContent(fields);
        var response = await _client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
}
```

---

## 脚本资产
- [scripts/query_transform.py](./scripts/query_transform.py) — 跨平台通用 Python 脚本，支持查询子级树与修改物体属性，无需第三方依赖库。
