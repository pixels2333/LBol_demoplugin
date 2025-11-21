using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LBoL.Base.Extensions
{
	// Token: 0x02000022 RID: 34
	public static class RecordLikeExtensions
	{
		// Token: 0x0600015E RID: 350 RVA: 0x00006362 File Offset: 0x00004562
		[MethodImpl(256)]
		public static Vector2 WithX(this Vector2 vector, float x)
		{
			return new Vector2(x, vector.y);
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00006370 File Offset: 0x00004570
		[MethodImpl(256)]
		public static Vector2 WithY(this Vector2 vector, float y)
		{
			return new Vector2(vector.x, y);
		}

		// Token: 0x06000160 RID: 352 RVA: 0x0000637E File Offset: 0x0000457E
		[MethodImpl(256)]
		public static Vector2 FlipX(this Vector2 vector)
		{
			return new Vector2(-vector.x, vector.y);
		}

		// Token: 0x06000161 RID: 353 RVA: 0x00006392 File Offset: 0x00004592
		[MethodImpl(256)]
		public static Vector2 FlipY(this Vector2 vector)
		{
			return new Vector2(vector.x, -vector.y);
		}

		// Token: 0x06000162 RID: 354 RVA: 0x000063A8 File Offset: 0x000045A8
		[MethodImpl(256)]
		public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
		{
			return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
		}

		// Token: 0x06000163 RID: 355 RVA: 0x00006408 File Offset: 0x00004608
		[MethodImpl(256)]
		public static Vector3 WithX(this Vector3 vector, float x)
		{
			return new Vector3(x, vector.y, vector.z);
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000641C File Offset: 0x0000461C
		[MethodImpl(256)]
		public static Vector3 WithY(this Vector3 vector, float y)
		{
			return new Vector3(vector.x, y, vector.z);
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00006430 File Offset: 0x00004630
		[MethodImpl(256)]
		public static Vector3 WithZ(this Vector3 vector, float z)
		{
			return new Vector3(vector.x, vector.y, z);
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00006444 File Offset: 0x00004644
		[MethodImpl(256)]
		public static Vector3 FlipX(this Vector3 vector)
		{
			return new Vector3(-vector.x, vector.y, vector.z);
		}

		// Token: 0x06000167 RID: 359 RVA: 0x0000645E File Offset: 0x0000465E
		[MethodImpl(256)]
		public static Vector3 FlipY(this Vector3 vector)
		{
			return new Vector3(vector.x, -vector.y, vector.z);
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00006478 File Offset: 0x00004678
		[MethodImpl(256)]
		public static Vector3 FlipZ(this Vector3 vector)
		{
			return new Vector3(vector.x, vector.y, -vector.z);
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00006494 File Offset: 0x00004694
		[MethodImpl(256)]
		public static Vector4 With(this Vector4 vector, float? x = null, float? y = null, float? z = null, float? w = null)
		{
			return new Vector4(x ?? vector.x, y ?? vector.y, z ?? vector.z, w ?? vector.w);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x0000650F File Offset: 0x0000470F
		[MethodImpl(256)]
		public static Vector4 WithX(this Vector4 vector, float x)
		{
			return new Vector4(x, vector.y, vector.z, vector.w);
		}

		// Token: 0x0600016B RID: 363 RVA: 0x00006529 File Offset: 0x00004729
		[MethodImpl(256)]
		public static Vector4 WithY(this Vector4 vector, float y)
		{
			return new Vector4(vector.x, y, vector.z, vector.w);
		}

		// Token: 0x0600016C RID: 364 RVA: 0x00006543 File Offset: 0x00004743
		[MethodImpl(256)]
		public static Vector4 WithZ(this Vector4 vector, float z)
		{
			return new Vector4(vector.x, vector.y, z, vector.w);
		}

		// Token: 0x0600016D RID: 365 RVA: 0x0000655D File Offset: 0x0000475D
		[MethodImpl(256)]
		public static Vector4 WithW(this Vector4 vector, float w)
		{
			return new Vector4(vector.x, vector.y, vector.z, w);
		}

		// Token: 0x0600016E RID: 366 RVA: 0x00006577 File Offset: 0x00004777
		[MethodImpl(256)]
		public static Vector4 FlipX(this Vector4 vector)
		{
			return new Vector4(-vector.x, vector.y, vector.z, vector.w);
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00006597 File Offset: 0x00004797
		[MethodImpl(256)]
		public static Vector4 FlipY(this Vector4 vector)
		{
			return new Vector4(vector.x, -vector.y, vector.z, vector.w);
		}

		// Token: 0x06000170 RID: 368 RVA: 0x000065B7 File Offset: 0x000047B7
		[MethodImpl(256)]
		public static Vector4 FlipZ(this Vector4 vector)
		{
			return new Vector4(vector.x, vector.y, -vector.z, vector.w);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x000065D7 File Offset: 0x000047D7
		[MethodImpl(256)]
		public static Vector4 FlipW(this Vector4 vector)
		{
			return new Vector4(vector.x, vector.y, vector.z, -vector.w);
		}

		// Token: 0x06000172 RID: 370 RVA: 0x000065F8 File Offset: 0x000047F8
		[MethodImpl(256)]
		public static Color With(this Color color, float? r = null, float? g = null, float? b = null, float? a = null)
		{
			return new Color(r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a);
		}

		// Token: 0x06000173 RID: 371 RVA: 0x00006673 File Offset: 0x00004873
		public static Color WithR(this Color color, float r)
		{
			return new Color(r, color.g, color.b, color.a);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000668D File Offset: 0x0000488D
		public static Color WithG(this Color color, float g)
		{
			return new Color(color.r, g, color.b, color.a);
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000066A7 File Offset: 0x000048A7
		public static Color WithB(this Color color, float b)
		{
			return new Color(color.r, color.g, b, color.a);
		}

		// Token: 0x06000176 RID: 374 RVA: 0x000066C1 File Offset: 0x000048C1
		public static Color WithA(this Color color, float a)
		{
			return new Color(color.r, color.g, color.b, a);
		}

		// Token: 0x06000177 RID: 375 RVA: 0x000066DC File Offset: 0x000048DC
		[MethodImpl(256)]
		public static Color32 With(this Color32 color, byte? r, byte? g, byte? b, byte? a)
		{
			return new Color32(r ?? color.r, g ?? color.g, b ?? color.b, a ?? color.a);
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00006757 File Offset: 0x00004957
		public static Color32 WithR(this Color32 color, byte r)
		{
			return new Color32(r, color.g, color.b, color.a);
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00006771 File Offset: 0x00004971
		public static Color32 WithG(this Color32 color, byte g)
		{
			return new Color32(color.r, g, color.b, color.a);
		}

		// Token: 0x0600017A RID: 378 RVA: 0x0000678B File Offset: 0x0000498B
		public static Color32 WithB(this Color32 color, byte b)
		{
			return new Color32(color.r, color.g, b, color.a);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x000067A5 File Offset: 0x000049A5
		public static Color32 WithA(this Color32 color, byte a)
		{
			return new Color32(color.r, color.g, color.b, a);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x000067C0 File Offset: 0x000049C0
		public static Rect With(this Rect rect, float? x = null, float? y = null, float? width = null, float? height = null)
		{
			return new Rect(x ?? rect.x, y ?? rect.y, width ?? rect.width, height ?? rect.height);
		}

		// Token: 0x0600017D RID: 381 RVA: 0x0000683F File Offset: 0x00004A3F
		public static Rect WithX(this Rect rect, float x)
		{
			return new Rect(x, rect.y, rect.width, rect.height);
		}

		// Token: 0x0600017E RID: 382 RVA: 0x0000685C File Offset: 0x00004A5C
		public static Rect WithY(this Rect rect, float y)
		{
			return new Rect(rect.x, y, rect.width, rect.height);
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00006879 File Offset: 0x00004A79
		public static Rect WithWidth(this Rect rect, float width)
		{
			return new Rect(rect.x, rect.y, width, rect.height);
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00006896 File Offset: 0x00004A96
		public static Rect WithHeight(this Rect rect, float height)
		{
			return new Rect(rect.x, rect.y, rect.width, height);
		}
	}
}
