#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Query Transform/RectTransform data from RuntimeUnityEditor HTTP API.
Usage: python query_transform.py <GameObjectName> [port]
Example: python query_transform.py Canvas/Button 9876
"""

import sys
import json
import urllib.request
import urllib.error


def query_transform(base_url, object_name):
    url = f"{base_url}/api/transform?name={urllib.request.quote(object_name)}"
    try:
        with urllib.request.urlopen(url, timeout=5) as response:
            data = json.loads(response.read().decode("utf-8"))
            print(json.dumps(data, indent=2, ensure_ascii=False))
            return data
    except urllib.error.HTTPError as e:
        print(f"HTTP Error {e.code}: {e.reason}")
    except urllib.error.URLError as e:
        print(f"Connection failed: {e.reason}")
    except Exception as e:
        print(f"Request failed: {e}")
    return None


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python query_transform.py <GameObjectName> [port]")
        print("Example: python query_transform.py Canvas/Button 9876")
        sys.exit(1)

    name = sys.argv[1]
    port = sys.argv[2] if len(sys.argv) > 2 else "9876"
    base_url = f"http://localhost:{port}"

    query_transform(base_url, name)
