using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace LBoL.Base.Extensions
{
	public static class MathExtensions
	{
		[MethodImpl(256)]
		public static int ToInt(this float value)
		{
			return (int)value;
		}
		[MethodImpl(256)]
		public static int ToInt(this double value)
		{
			return (int)value;
		}
		[MethodImpl(256)]
		public static float ToFloat(this double value)
		{
			return (float)value;
		}
		[MethodImpl(256)]
		public static int Abs(this int value)
		{
			return Math.Abs(value);
		}
		[MethodImpl(256)]
		public static float Abs(this float value)
		{
			return Math.Abs(value);
		}
		[MethodImpl(256)]
		public static double Abs(this double value)
		{
			return Math.Abs(value);
		}
		[MethodImpl(256)]
		public static bool Approximately(this float a, float b)
		{
			return Mathf.Approximately(a, b);
		}
		[MethodImpl(256)]
		public static bool Approximately(this double a, double b)
		{
			return Math.Abs(b - a) < Math.Max(1E-06 * Math.Max(Math.Abs(a), Math.Abs(b)), 4E-323);
		}
		[MethodImpl(256)]
		public static float Round(this float value)
		{
			return Math.Round((double)value).ToFloat();
		}
		[MethodImpl(256)]
		public static float Round(this float value, MidpointRounding mode)
		{
			return Math.Round((double)value, mode).ToFloat();
		}
		[MethodImpl(256)]
		public static double Round(this double value)
		{
			return Math.Round(value);
		}
		[MethodImpl(256)]
		public static double Round(this double value, MidpointRounding mode)
		{
			return Math.Round(value, mode);
		}
		[MethodImpl(256)]
		public static int RoundToInt(this float value)
		{
			return Math.Round((double)value).ToInt();
		}
		[MethodImpl(256)]
		public static int RoundToInt(this float value, MidpointRounding mode)
		{
			return Math.Round((double)value, mode).ToInt();
		}
		[MethodImpl(256)]
		public static int RoundToInt(this double value)
		{
			return Math.Round(value).ToInt();
		}
		[MethodImpl(256)]
		public static int RoundToInt(this double value, MidpointRounding mode)
		{
			return Math.Round(value, mode).ToInt();
		}
		[MethodImpl(256)]
		public static float Ceiling(this float value)
		{
			return Math.Ceiling((double)value).ToFloat();
		}
		[MethodImpl(256)]
		public static double Ceiling(this double value)
		{
			return Math.Ceiling(value);
		}
		[MethodImpl(256)]
		public static int CeilingToInt(this float value)
		{
			return Math.Ceiling((double)value).ToInt();
		}
		[MethodImpl(256)]
		public static int CeilingToInt(this double value)
		{
			return Math.Ceiling(value).ToInt();
		}
		[MethodImpl(256)]
		public static float Floor(this float value)
		{
			return Math.Floor((double)value).ToFloat();
		}
		[MethodImpl(256)]
		public static double Floor(this double value)
		{
			return Math.Floor(value);
		}
		[MethodImpl(256)]
		public static int FloorToInt(this float value)
		{
			return Math.Floor((double)value).ToInt();
		}
		[MethodImpl(256)]
		public static int FloorToInt(this double value)
		{
			return Math.Floor(value).ToInt();
		}
		[MethodImpl(256)]
		public static float Truncate(this float value)
		{
			return Convert.ToSingle(Math.Truncate((double)value));
		}
		[MethodImpl(256)]
		public static double Truncate(this double value)
		{
			return Math.Truncate(value);
		}
		[MethodImpl(256)]
		public static int TruncateToInt(this float value)
		{
			return Convert.ToInt32(Math.Truncate((double)value));
		}
		[MethodImpl(256)]
		public static int TruncateToInt(this double value)
		{
			return Convert.ToInt32(Math.Truncate(value));
		}
		[MethodImpl(256)]
		public static int Sign(this int value)
		{
			return Math.Sign(value);
		}
		[MethodImpl(256)]
		public static int Sign(this float value)
		{
			return Math.Sign(value);
		}
		[MethodImpl(256)]
		public static int Sign(this double value)
		{
			return Math.Sign(value);
		}
		[MethodImpl(256)]
		public static int Clamp(this int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}
			if (value <= max)
			{
				return value;
			}
			return max;
		}
		[MethodImpl(256)]
		public static float Clamp(this float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}
		[MethodImpl(256)]
		public static double Clamp(this double value, double min, double max)
		{
			if (value < min)
			{
				return min;
			}
			if (value <= max)
			{
				return value;
			}
			return max;
		}
		[MethodImpl(256)]
		public static float Clamp01(this float value)
		{
			return Mathf.Clamp01(value);
		}
		[MethodImpl(256)]
		public static double Clamp01(this double value)
		{
			if (value < 0.0)
			{
				return 0.0;
			}
			if (value <= 1.0)
			{
				return value;
			}
			return 1.0;
		}
		[MethodImpl(256)]
		public static int Square(this int value)
		{
			return value * value;
		}
		[MethodImpl(256)]
		public static float Square(this float value)
		{
			return value * value;
		}
		[MethodImpl(256)]
		public static double Square(this double value)
		{
			return value * value;
		}
		[MethodImpl(256)]
		public static float Sqrt(this float value)
		{
			return Math.Sqrt((double)value).ToFloat();
		}
		[MethodImpl(256)]
		public static double Sqrt(this double value)
		{
			return Math.Sqrt(value);
		}
		[MethodImpl(256)]
		public static float Hypot(float a, float b)
		{
			return (a * a + b * b).Sqrt();
		}
		[MethodImpl(256)]
		public static double Hypot(double a, double b)
		{
			return (a * a + b * b).Sqrt();
		}
		[MethodImpl(256)]
		public static float ToRadian(this float value)
		{
			return 0.017453292f * value;
		}
		[MethodImpl(256)]
		public static float ToDegree(this float value)
		{
			return 57.29578f * value;
		}
		[MethodImpl(256)]
		public static float Lerp(this float t, float a, float b)
		{
			return Mathf.Lerp(a, b, t);
		}
		[MethodImpl(256)]
		public static Vector2 Lerp(this float t, Vector2 a, Vector2 b)
		{
			return Vector2.Lerp(a, b, t);
		}
		[MethodImpl(256)]
		public static Vector3 Lerp(this float t, Vector3 a, Vector3 b)
		{
			return Vector3.Lerp(a, b, t);
		}
		[MethodImpl(256)]
		public static Vector4 Lerp(this float t, Vector4 a, Vector4 b)
		{
			return Vector4.Lerp(a, b, t);
		}
		[MethodImpl(256)]
		public static Quaternion Lerp(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.Lerp(a, b, t);
		}
		[MethodImpl(256)]
		public static Quaternion Slerp(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.Slerp(a, b, t);
		}
		[MethodImpl(256)]
		public static float LerpUnclamped(this float t, float a, float b)
		{
			return Mathf.LerpUnclamped(a, b, t);
		}
		[MethodImpl(256)]
		public static Vector2 LerpUnclamped(this float t, Vector2 a, Vector2 b)
		{
			return Vector2.LerpUnclamped(a, b, t);
		}
		[MethodImpl(256)]
		public static Vector3 LerpUnclamped(this float t, Vector3 a, Vector3 b)
		{
			return Vector3.LerpUnclamped(a, b, t);
		}
		[MethodImpl(256)]
		public static Vector4 LerpUnclamped(this float t, Vector4 a, Vector4 b)
		{
			return Vector4.LerpUnclamped(a, b, t);
		}
		[MethodImpl(256)]
		public static Quaternion LerpUnclamped(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.LerpUnclamped(a, b, t);
		}
		[MethodImpl(256)]
		public static Quaternion SlerpUnclamped(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.SlerpUnclamped(a, b, t);
		}
		[MethodImpl(256)]
		public static float InverseLerp(this float value, float a, float b)
		{
			return Mathf.InverseLerp(a, b, value);
		}
		[MethodImpl(256)]
		public static float LerpAngle(this float t, float a, float b)
		{
			return Mathf.LerpAngle(a, b, t);
		}
		[MethodImpl(256)]
		[return: TupleElementNames(new string[] { "result", "remainder" })]
		public static ValueTuple<int, int> DivRem(this int a, int b)
		{
			return new ValueTuple<int, int>(a / b, a % b);
		}
		[MethodImpl(256)]
		public static void Deconstruct(this Vector2 v, out float x, out float y)
		{
			x = v.x;
			y = v.y;
		}
		[MethodImpl(256)]
		public static void Deconstruct(this Vector2Int v, out int x, out int y)
		{
			x = v.x;
			y = v.y;
		}
		[MethodImpl(256)]
		public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}
		[MethodImpl(256)]
		public static void Deconstruct(this Vector3Int v, out int x, out int y, out int z)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}
		[MethodImpl(256)]
		public static void Deconstruct(this Vector4 v, out float x, out float y, out float z, out float w)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			w = v.w;
		}
		[MethodImpl(256)]
		public static void Deconstruct(this Rect rect, out float x, out float y, out float width, out float height)
		{
			x = rect.x;
			y = rect.y;
			width = rect.width;
			height = rect.height;
		}
	}
}
