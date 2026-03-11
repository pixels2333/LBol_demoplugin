using System.Collections.Generic;

namespace UiObjectQueryPlugin.Models;

public sealed class ObjectQueryRequest
{
    public string Name { get; set; }

    public bool? IncludeInactive { get; set; }

    public int? MaxResults { get; set; }
}

public sealed class ObjectQueryResponse
{
    public bool Success { get; set; }

    public string Error { get; set; }

    public string Name { get; set; }

    public int Count { get; set; }

    public bool IncludeInactive { get; set; }

    public IReadOnlyList<ObjectSnapshot> Matches { get; set; } = new List<ObjectSnapshot>();
}

public sealed class ObjectSnapshot
{
    public int InstanceId { get; set; }

    public string Name { get; set; }

    public string Path { get; set; }

    public string SceneName { get; set; }

    public bool ActiveSelf { get; set; }

    public bool ActiveInHierarchy { get; set; }

    public string Tag { get; set; }

    public int Layer { get; set; }

    public TransformSnapshot Transform { get; set; }

    public RectTransformSnapshot RectTransform { get; set; }
}

public sealed class TransformSnapshot
{
    public Vector3Snapshot Position { get; set; }

    public Vector3Snapshot LocalPosition { get; set; }

    public Vector3Snapshot LocalScale { get; set; }

    public Vector3Snapshot LossyScale { get; set; }

    public Vector3Snapshot EulerAngles { get; set; }

    public int SiblingIndex { get; set; }

    public int ChildCount { get; set; }
}

public sealed class RectTransformSnapshot
{
    public Vector2Snapshot AnchoredPosition { get; set; }

    public Vector2Snapshot SizeDelta { get; set; }

    public Vector2Snapshot AnchorMin { get; set; }

    public Vector2Snapshot AnchorMax { get; set; }

    public Vector2Snapshot Pivot { get; set; }

    public Vector2Snapshot OffsetMin { get; set; }

    public Vector2Snapshot OffsetMax { get; set; }
}

public sealed class Vector3Snapshot
{
    public float X { get; set; }

    public float Y { get; set; }

    public float Z { get; set; }
}

public sealed class Vector2Snapshot
{
    public float X { get; set; }

    public float Y { get; set; }
}
