using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace LBoL.Base.Extensions
{
	public static class RecordLikeExtensions
	{
		[MethodImpl(256)]
		public static Vector2 WithX(this Vector2 vector, float x)
		{
			return new Vector2(x, vector.y);
		}
		[MethodImpl(256)]
		public static Vector2 WithY(this Vector2 vector, float y)
		{
			return new Vector2(vector.x, y);
		}
		[MethodImpl(256)]
		public static Vector2 FlipX(this Vector2 vector)
		{
			return new Vector2(-vector.x, vector.y);
		}
		[MethodImpl(256)]
		public static Vector2 FlipY(this Vector2 vector)
		{
			return new Vector2(vector.x, -vector.y);
		}
		[MethodImpl(256)]
		public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
		{
			return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
		}
		[MethodImpl(256)]
		public static Vector3 WithX(this Vector3 vector, float x)
		{
			return new Vector3(x, vector.y, vector.z);
		}
		[MethodImpl(256)]
		public static Vector3 WithY(this Vector3 vector, float y)
		{
			return new Vector3(vector.x, y, vector.z);
		}
		[MethodImpl(256)]
		public static Vector3 WithZ(this Vector3 vector, float z)
		{
			return new Vector3(vector.x, vector.y, z);
		}
		[MethodImpl(256)]
		public static Vector3 FlipX(this Vector3 vector)
		{
			return new Vector3(-vector.x, vector.y, vector.z);
		}
		[MethodImpl(256)]
		public static Vector3 FlipY(this Vector3 vector)
		{
			return new Vector3(vector.x, -vector.y, vector.z);
		}
		[MethodImpl(256)]
		public static Vector3 FlipZ(this Vector3 vector)
		{
			return new Vector3(vector.x, vector.y, -vector.z);
		}
		[MethodImpl(256)]
		public static Vector4 With(this Vector4 vector, float? x = null, float? y = null, float? z = null, float? w = null)
		{
			return new Vector4(x ?? vector.x, y ?? vector.y, z ?? vector.z, w ?? vector.w);
		}
		[MethodImpl(256)]
		public static Vector4 WithX(this Vector4 vector, float x)
		{
			return new Vector4(x, vector.y, vector.z, vector.w);
		}
		[MethodImpl(256)]
		public static Vector4 WithY(this Vector4 vector, float y)
		{
			return new Vector4(vector.x, y, vector.z, vector.w);
		}
		[MethodImpl(256)]
		public static Vector4 WithZ(this Vector4 vector, float z)
		{
			return new Vector4(vector.x, vector.y, z, vector.w);
		}
		[MethodImpl(256)]
		public static Vector4 WithW(this Vector4 vector, float w)
		{
			return new Vector4(vector.x, vector.y, vector.z, w);
		}
		[MethodImpl(256)]
		public static Vector4 FlipX(this Vector4 vector)
		{
			return new Vector4(-vector.x, vector.y, vector.z, vector.w);
		}
		[MethodImpl(256)]
		public static Vector4 FlipY(this Vector4 vector)
		{
			return new Vector4(vector.x, -vector.y, vector.z, vector.w);
		}
		[MethodImpl(256)]
		public static Vector4 FlipZ(this Vector4 vector)
		{
			return new Vector4(vector.x, vector.y, -vector.z, vector.w);
		}
		[MethodImpl(256)]
		public static Vector4 FlipW(this Vector4 vector)
		{
			return new Vector4(vector.x, vector.y, vector.z, -vector.w);
		}
		[MethodImpl(256)]
		public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
		{
			return new Color(r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a);
		}
		public static Color WithR(this Color color, float r)
		{
			return new Color(r, color.g, color.b, color.a);
		}
		public static Color WithG(this Color color, float g)
		{
			return new Color(color.r, g, color.b, color.a);
		}
		public static Color WithB(this Color color, float b)
		{
			return new Color(color.r, color.g, b, color.a);
		}
		public static Color WithA(this Color color, float a)
		{
			return new Color(color.r, color.g, color.b, a);
		}
		[MethodImpl(256)]
		public static Color32 With(this Color32 color, byte? r, byte? g, byte? b, byte? a)
		{
			return new Color32(r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a);
		}
		public static Color32 WithR(this Color32 color, byte r)
		{
			return new Color32(r, color.g, color.b, color.a);
		}
		public static Color32 WithG(this Color32 color, byte g)
		{
			return new Color32(color.r, g, color.b, color.a);
		}
		public static Color32 WithB(this Color32 color, byte b)
		{
			return new Color32(color.r, color.g, b, color.a);
		}
		public static Color32 WithA(this Color32 color, byte a)
		{
			return new Color32(color.r, color.g, color.b, a);
		}
		public static Rect With(this Rect rect, float? x = null, float? y = null, float? width = null, float? height = null)
		{
			return new Rect(x ?? rect.x, y ?? rect.y, width ?? rect.width, height ?? rect.height);
		}
		public static Rect WithX(this Rect rect, float x)
		{
			return new Rect(x, rect.y, rect.width, rect.height);
		}
		public static Rect WithY(this Rect rect, float y)
		{
			return new Rect(rect.x, y, rect.width, rect.height);
		}
		public static Rect WithWidth(this Rect rect, float width)
		{
			return new Rect(rect.x, rect.y, width, rect.height);
		}
		public static Rect WithHeight(this Rect rect, float height)
		{
			return new Rect(rect.x, rect.y, rect.width, height);
		}
	}
}
