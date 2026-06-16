#!/usr/bin/env python3
import urllib.request, urllib.parse, json

port = 9876
names = ["RemoteIcon_aidefault", "RemoteIcon_aidefault2"]

for name in names:
    url = f"http://localhost:{port}/api/transform?name={urllib.parse.quote(name)}"
    try:
        data = json.loads(urllib.request.urlopen(url, timeout=3).read().decode())
        if "error" not in data:
            rt = data.get("rectTransform", {})
            pos = rt.get("anchoredPosition", {})
            size = rt.get("sizeDelta", {})
            print(f"{name}:")
            print(f"  anchoredPosition: ({pos.get('x',0):.1f}, {pos.get('y',0):.1f})")
            print(f"  sizeDelta: ({size.get('x',0):.1f}, {size.get('y',0):.1f})")
            print(f"  active: {data.get('active')}")
    except Exception as e:
        print(f"Error querying {name}: {e}")

# Now try to find local player with various IDs
# The self player ID might be a network-assigned ID
local_candidates = ["__local__", "local", "self", "player", "host", "0", "1", "2", "3", "aidefault3", "aidefault4"]
for pid in local_candidates:
    icon_name = f"RemoteIcon_{pid}"
    url = f"http://localhost:{port}/api/transform?name={urllib.parse.quote(icon_name)}"
    try:
        data = json.loads(urllib.request.urlopen(url, timeout=2).read().decode())
        if "error" not in data:
            rt = data.get("rectTransform", {})
            pos = rt.get("anchoredPosition", {})
            print(f"\nFOUND LOCAL: {icon_name}")
            print(f"  anchoredPosition: ({pos.get('x',0):.1f}, {pos.get('y',0):.1f})")
            print(f"  active: {data.get('active')}")
    except:
        pass
