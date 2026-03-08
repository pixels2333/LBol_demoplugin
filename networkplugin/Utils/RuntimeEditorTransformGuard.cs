using System;
using UnityEngine;

namespace NetworkPlugin.Utils
{
    internal static class RuntimeEditorTransformGuard
    {
        public static bool ApplyRectTransform(RectTransform rectTransform, Action<RectTransform> apply)
        {
            if (rectTransform == null || apply == null)
            {
                return false;
            }

            RuntimeEditorTransformGuardState state = RuntimeEditorTransformGuardState.GetOrAdd(rectTransform.gameObject);
            return state.TryApplyRectTransform(rectTransform, apply);
        }

        public static bool ApplyTransform(Transform transform, Action<Transform> apply)
        {
            if (transform == null || apply == null)
            {
                return false;
            }

            RuntimeEditorTransformGuardState state = RuntimeEditorTransformGuardState.GetOrAdd(transform.gameObject);
            return state.TryApplyTransform(transform, apply);
        }
    }

    [DisallowMultipleComponent]
    internal sealed class RuntimeEditorTransformGuardState : MonoBehaviour
    {
        private bool _hasRectSnapshot;
        private int _lastRectParentId;
        private RectTransformSnapshot _lastRectSnapshot;
        private bool _rectManualOverride;

        private bool _hasTransformSnapshot;
        private int _lastTransformParentId;
        private TransformSnapshot _lastTransformSnapshot;
        private bool _transformManualOverride;

        public static RuntimeEditorTransformGuardState GetOrAdd(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            RuntimeEditorTransformGuardState state = gameObject.GetComponent<RuntimeEditorTransformGuardState>();
            return state ?? gameObject.AddComponent<RuntimeEditorTransformGuardState>();
        }

        public bool TryApplyRectTransform(RectTransform rectTransform, Action<RectTransform> apply)
        {
            if (rectTransform == null || apply == null)
            {
                return false;
            }

            int currentParentId = rectTransform.parent != null ? rectTransform.parent.GetInstanceID() : 0;
            if (_rectManualOverride)
            {
                return false;
            }

            if (_hasRectSnapshot && _lastRectParentId == currentParentId)
            {
                RectTransformSnapshot current = RectTransformSnapshot.Capture(rectTransform);
                if (!current.Approximately(_lastRectSnapshot))
                {
                    _rectManualOverride = true;
                    return false;
                }
            }

            apply(rectTransform);

            _lastRectSnapshot = RectTransformSnapshot.Capture(rectTransform);
            _lastRectParentId = currentParentId;
            _hasRectSnapshot = true;
            return true;
        }

        public bool TryApplyTransform(Transform transform, Action<Transform> apply)
        {
            if (transform == null || apply == null)
            {
                return false;
            }

            int currentParentId = transform.parent != null ? transform.parent.GetInstanceID() : 0;
            if (_transformManualOverride)
            {
                return false;
            }

            if (_hasTransformSnapshot && _lastTransformParentId == currentParentId)
            {
                TransformSnapshot current = TransformSnapshot.Capture(transform);
                if (!current.Approximately(_lastTransformSnapshot))
                {
                    _transformManualOverride = true;
                    return false;
                }
            }

            apply(transform);

            _lastTransformSnapshot = TransformSnapshot.Capture(transform);
            _lastTransformParentId = currentParentId;
            _hasTransformSnapshot = true;
            return true;
        }
    }

    internal readonly struct RectTransformSnapshot
    {
        private const float Vector2Tolerance = 0.05f;
        private const float Vector3Tolerance = 0.05f;

        private RectTransformSnapshot(
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Vector3 localPosition,
            Vector3 localScale,
            Vector3 localEulerAngles)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            Pivot = pivot;
            AnchoredPosition = anchoredPosition;
            SizeDelta = sizeDelta;
            OffsetMin = offsetMin;
            OffsetMax = offsetMax;
            LocalPosition = localPosition;
            LocalScale = localScale;
            LocalEulerAngles = localEulerAngles;
        }

        public Vector2 AnchorMin { get; }
        public Vector2 AnchorMax { get; }
        public Vector2 Pivot { get; }
        public Vector2 AnchoredPosition { get; }
        public Vector2 SizeDelta { get; }
        public Vector2 OffsetMin { get; }
        public Vector2 OffsetMax { get; }
        public Vector3 LocalPosition { get; }
        public Vector3 LocalScale { get; }
        public Vector3 LocalEulerAngles { get; }

        public static RectTransformSnapshot Capture(RectTransform rectTransform)
        {
            return new RectTransformSnapshot(
                rectTransform.anchorMin,
                rectTransform.anchorMax,
                rectTransform.pivot,
                rectTransform.anchoredPosition,
                rectTransform.sizeDelta,
                rectTransform.offsetMin,
                rectTransform.offsetMax,
                rectTransform.localPosition,
                rectTransform.localScale,
                rectTransform.localEulerAngles);
        }

        public bool Approximately(RectTransformSnapshot other)
        {
            return Approximately(AnchorMin, other.AnchorMin, Vector2Tolerance)
                && Approximately(AnchorMax, other.AnchorMax, Vector2Tolerance)
                && Approximately(Pivot, other.Pivot, Vector2Tolerance)
                && Approximately(AnchoredPosition, other.AnchoredPosition, Vector2Tolerance)
                && Approximately(SizeDelta, other.SizeDelta, Vector2Tolerance)
                && Approximately(OffsetMin, other.OffsetMin, Vector2Tolerance)
                && Approximately(OffsetMax, other.OffsetMax, Vector2Tolerance)
                && Approximately(LocalPosition, other.LocalPosition, Vector3Tolerance)
                && Approximately(LocalScale, other.LocalScale, Vector3Tolerance)
                && Approximately(LocalEulerAngles, other.LocalEulerAngles, Vector3Tolerance);
        }

        private static bool Approximately(Vector2 left, Vector2 right, float tolerance)
        {
            return (left - right).sqrMagnitude <= tolerance * tolerance;
        }

        private static bool Approximately(Vector3 left, Vector3 right, float tolerance)
        {
            return (left - right).sqrMagnitude <= tolerance * tolerance;
        }
    }

    internal readonly struct TransformSnapshot
    {
        private const float Vector3Tolerance = 0.05f;

        private TransformSnapshot(Vector3 localPosition, Vector3 localScale, Vector3 localEulerAngles)
        {
            LocalPosition = localPosition;
            LocalScale = localScale;
            LocalEulerAngles = localEulerAngles;
        }

        public Vector3 LocalPosition { get; }
        public Vector3 LocalScale { get; }
        public Vector3 LocalEulerAngles { get; }

        public static TransformSnapshot Capture(Transform transform)
        {
            return new TransformSnapshot(transform.localPosition, transform.localScale, transform.localEulerAngles);
        }

        public bool Approximately(TransformSnapshot other)
        {
            return Approximately(LocalPosition, other.LocalPosition)
                && Approximately(LocalScale, other.LocalScale)
                && Approximately(LocalEulerAngles, other.LocalEulerAngles);
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return (left - right).sqrMagnitude <= Vector3Tolerance * Vector3Tolerance;
        }
    }
}