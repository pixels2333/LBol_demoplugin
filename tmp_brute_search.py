#!/usr/bin/env python3
"""Brute force search for RemoteIcon objects."""
import urllib.request, urllib.parse, json

port = 9876

# Try many numeric IDs and common patterns
candidates = [str(i) for i in range(50)]
candidates += [f"player{i}" for i in range(10)]
candidates += [f"LocalPlayer", f"localplayer", f"Local", f"local"]
candidates += [f"host", f"Host", f"HostPlayer"]
candidates += [f"steam_{i}" for i in range(5)]

found = []
for pid in candidates:
    icon_name = f"RemoteIcon_{pid}"
    url = f"http://localhost:{port}/api/transform?name={urllib.parse.quote(icon_name)}"
    try:
        data = json.loads(urllib.request.urlopen(url, timeout=1).read().decode())
        if "error" not in data:
            rt = data.get("rectTransform", {})
            pos = rt.get("anchoredPosition", {})
            found.append((icon_name, pos.get("x", 0), pos.get("y", 0)))
            print(f"FOUND: {icon_name} -> ({pos.get('x',0):.1f}, {pos.get('y',0):.1f})")
    except:
        pass

if not found:
    print("No additional icons found")
else:
    print(f"\nTotal found: {len(found)}")
