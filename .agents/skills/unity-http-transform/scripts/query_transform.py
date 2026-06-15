#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Query / Set Transform/RectTransform data from RuntimeUnityEditor HTTP API.

Query:
    python query_transform.py <GameObjectName> [port]

Set (form-urlencoded body):
    python query_transform.py --set <GameObjectName> [port] [key=value ...]
    python query_transform.py --set Button 9876 transform.position.x=100 rectTransform.sizeDelta.x=200
"""

import sys
import json
import urllib.request
import urllib.error
import urllib.parse


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


def set_transform(base_url, object_name, fields):
    url = f"{base_url}/api/transform/set?name={urllib.request.quote(object_name)}"
    body = urllib.parse.urlencode(fields)
    data_bytes = body.encode("utf-8")
    req = urllib.request.Request(url, data=data_bytes, method="POST",
                                  headers={"Content-Type": "application/x-www-form-urlencoded"})
    try:
        with urllib.request.urlopen(req, timeout=5) as response:
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
    args = sys.argv[1:]
    if not args:
        print("Usage:")
        print("  Query : python query_transform.py <GameObjectName> [port]")
        print("  Set   : python query_transform.py --set <GameObjectName> [port] [key=value ...]")
        print("Example:")
        print('  python query_transform.py --set Button 9876 transform.position.x=100 rectTransform.sizeDelta.x=200')
        sys.exit(1)

    if args[0] == "--set":
        if len(args) < 2:
            print("Missing GameObjectName for --set")
            sys.exit(1)
        name = args[1]
        port = "9876"
        fields = {}
        for arg in args[2:]:
            if arg.isdigit():
                port = arg
            elif "=" in arg:
                k, v = arg.split("=", 1)
                fields[k] = v
        base_url = f"http://localhost:{port}"
        set_transform(base_url, name, fields)
    else:
        name = args[0]
        port = args[1] if len(args) > 1 else "9876"
        base_url = f"http://localhost:{port}"
        query_transform(base_url, name)
