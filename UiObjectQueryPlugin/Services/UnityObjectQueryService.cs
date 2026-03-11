using System;
using System.Collections.Generic;
using System.Linq;
using UiObjectQueryPlugin.Models;
using UnityEngine;

namespace UiObjectQueryPlugin.Services;

public sealed class UnityObjectQueryService
{
    private readonly int _defaultMaxResults;
    private readonly bool _defaultIncludeInactive;

    public UnityObjectQueryService(int defaultMaxResults, bool defaultIncludeInactive)
    {
        _defaultMaxResults = Math.Max(1, defaultMaxResults);
        _defaultIncludeInactive = defaultIncludeInactive;
    }

    public ObjectQueryResponse Query(ObjectQueryRequest request)
    {
        string name = request?.Name?.Trim();
        bool includeInactive = request?.IncludeInactive ?? _defaultIncludeInactive;
        int maxResults = Math.Max(1, request?.MaxResults ?? _defaultMaxResults);

        if (string.IsNullOrWhiteSpace(name))
        {
            return new ObjectQueryResponse
            {
                Success = false,
                Error = "`name` 不能为空。",
                Name = request?.Name ?? string.Empty,
                IncludeInactive = includeInactive,
            };
        }

        List<ObjectSnapshot> matches = Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(IsSceneObject)
            .Where(gameObject => includeInactive || gameObject.activeInHierarchy)
            .Where(gameObject => string.Equals(gameObject.name, name, StringComparison.Ordinal))
            .OrderBy(gameObject => BuildPath(gameObject.transform), StringComparer.Ordinal)
            .Take(maxResults)
            .Select(MapSnapshot)
            .ToList();

        return new ObjectQueryResponse
        {
            Success = true,
            Name = name,
            Count = matches.Count,
            IncludeInactive = includeInactive,
            Matches = matches,
        };
    }

    private static bool IsSceneObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        if (!gameObject.scene.IsValid())
        {
            return false;
        }

        return (gameObject.hideFlags & HideFlags.HideAndDontSave) == 0;
    }

    private static ObjectSnapshot MapSnapshot(GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        RectTransform rectTransform = transform as RectTransform;

        return new ObjectSnapshot
        {
            InstanceId = gameObject.GetInstanceID(),
            Name = gameObject.name,
            Path = BuildPath(transform),
            SceneName = gameObject.scene.name,
            ActiveSelf = gameObject.activeSelf,
            ActiveInHierarchy = gameObject.activeInHierarchy,
            Tag = gameObject.tag,
            Layer = gameObject.layer,
            Transform = new TransformSnapshot
            {
                Position = ToVector3(transform.position),
                LocalPosition = ToVector3(transform.localPosition),
                LocalScale = ToVector3(transform.localScale),
                LossyScale = ToVector3(transform.lossyScale),
                EulerAngles = ToVector3(transform.eulerAngles),
                SiblingIndex = transform.GetSiblingIndex(),
                ChildCount = transform.childCount,
            },
            RectTransform = rectTransform == null
                ? null
                : new RectTransformSnapshot
                {
                    AnchoredPosition = ToVector2(rectTransform.anchoredPosition),
                    SizeDelta = ToVector2(rectTransform.sizeDelta),
                    AnchorMin = ToVector2(rectTransform.anchorMin),
                    AnchorMax = ToVector2(rectTransform.anchorMax),
                    Pivot = ToVector2(rectTransform.pivot),
                    OffsetMin = ToVector2(rectTransform.offsetMin),
                    OffsetMax = ToVector2(rectTransform.offsetMax),
                },
        };
    }

    private static string BuildPath(Transform transform)
    {
        List<string> segments = new List<string>();
        Transform current = transform;
        while (current != null)
        {
            segments.Add(current.name);
            current = current.parent;
        }

        segments.Reverse();
        return string.Join("/", segments);
    }

    private static Vector3Snapshot ToVector3(Vector3 value)
    {
        return new Vector3Snapshot
        {
            X = value.x,
            Y = value.y,
            Z = value.z,
        };
    }

    private static Vector2Snapshot ToVector2(Vector2 value)
    {
        return new Vector2Snapshot
        {
            X = value.x,
            Y = value.y,
        };
    }
}
