using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LBoL.Base.Extensions
{
	// Token: 0x02000021 RID: 33
	public static class MathExtensions
	{
		// Token: 0x0600011C RID: 284 RVA: 0x00005FFD File Offset: 0x000041FD
		[MethodImpl(256)]
		public static int ToInt(this float value)
		{
			return (int)value;
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00006001 File Offset: 0x00004201
		[MethodImpl(256)]
		public static int ToInt(this double value)
		{
			return (int)value;
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00006005 File Offset: 0x00004205
		[MethodImpl(256)]
		public static float ToFloat(this double value)
		{
			return (float)value;
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00006009 File Offset: 0x00004209
		[MethodImpl(256)]
		public static int Abs(this int value)
		{
			return Math.Abs(value);
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00006011 File Offset: 0x00004211
		[MethodImpl(256)]
		public static float Abs(this float value)
		{
			return Math.Abs(value);
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00006019 File Offset: 0x00004219
		[MethodImpl(256)]
		public static double Abs(this double value)
		{
			return Math.Abs(value);
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00006021 File Offset: 0x00004221
		[MethodImpl(256)]
		public static bool Approximately(this float a, float b)
		{
			return Mathf.Approximately(a, b);
		}

		// Token: 0x06000123 RID: 291 RVA: 0x0000602A File Offset: 0x0000422A
		[MethodImpl(256)]
		public static bool Approximately(this double a, double b)
		{
			return Math.Abs(b - a) < Math.Max(1E-06 * Math.Max(Math.Abs(a), Math.Abs(b)), 4E-323);
		}

		// Token: 0x06000124 RID: 292 RVA: 0x0000605F File Offset: 0x0000425F
		[MethodImpl(256)]
		public static float Round(this float value)
		{
			return Math.Round((double)value).ToFloat();
		}

		// Token: 0x06000125 RID: 293 RVA: 0x0000606D File Offset: 0x0000426D
		[MethodImpl(256)]
		public static float Round(this float value, MidpointRounding mode)
		{
			return Math.Round((double)value, mode).ToFloat();
		}

		// Token: 0x06000126 RID: 294 RVA: 0x0000607C File Offset: 0x0000427C
		[MethodImpl(256)]
		public static double Round(this double value)
		{
			return Math.Round(value);
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00006084 File Offset: 0x00004284
		[MethodImpl(256)]
		public static double Round(this double value, MidpointRounding mode)
		{
			return Math.Round(value, mode);
		}

		// Token: 0x06000128 RID: 296 RVA: 0x0000608D File Offset: 0x0000428D
		[MethodImpl(256)]
		public static int RoundToInt(this float value)
		{
			return Math.Round((double)value).ToInt();
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0000609B File Offset: 0x0000429B
		[MethodImpl(256)]
		public static int RoundToInt(this float value, MidpointRounding mode)
		{
			return Math.Round((double)value, mode).ToInt();
		}

		// Token: 0x0600012A RID: 298 RVA: 0x000060AA File Offset: 0x000042AA
		[MethodImpl(256)]
		public static int RoundToInt(this double value)
		{
			return Math.Round(value).ToInt();
		}

		// Token: 0x0600012B RID: 299 RVA: 0x000060B7 File Offset: 0x000042B7
		[MethodImpl(256)]
		public static int RoundToInt(this double value, MidpointRounding mode)
		{
			return Math.Round(value, mode).ToInt();
		}

		// Token: 0x0600012C RID: 300 RVA: 0x000060C5 File Offset: 0x000042C5
		[MethodImpl(256)]
		public static float Ceiling(this float value)
		{
			return Math.Ceiling((double)value).ToFloat();
		}

		// Token: 0x0600012D RID: 301 RVA: 0x000060D3 File Offset: 0x000042D3
		[MethodImpl(256)]
		public static double Ceiling(this double value)
		{
			return Math.Ceiling(value);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x000060DB File Offset: 0x000042DB
		[MethodImpl(256)]
		public static int CeilingToInt(this float value)
		{
			return Math.Ceiling((double)value).ToInt();
		}

		// Token: 0x0600012F RID: 303 RVA: 0x000060E9 File Offset: 0x000042E9
		[MethodImpl(256)]
		public static int CeilingToInt(this double value)
		{
			return Math.Ceiling(value).ToInt();
		}

		// Token: 0x06000130 RID: 304 RVA: 0x000060F6 File Offset: 0x000042F6
		[MethodImpl(256)]
		public static float Floor(this float value)
		{
			return Math.Floor((double)value).ToFloat();
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00006104 File Offset: 0x00004304
		[MethodImpl(256)]
		public static double Floor(this double value)
		{
			return Math.Floor(value);
		}

		// Token: 0x06000132 RID: 306 RVA: 0x0000610C File Offset: 0x0000430C
		[MethodImpl(256)]
		public static int FloorToInt(this float value)
		{
			return Math.Floor((double)value).ToInt();
		}

		// Token: 0x06000133 RID: 307 RVA: 0x0000611A File Offset: 0x0000431A
		[MethodImpl(256)]
		public static int FloorToInt(this double value)
		{
			return Math.Floor(value).ToInt();
		}

		// Token: 0x06000134 RID: 308 RVA: 0x00006127 File Offset: 0x00004327
		[MethodImpl(256)]
		public static float Truncate(this float value)
		{
			return Convert.ToSingle(Math.Truncate((double)value));
		}

		// Token: 0x06000135 RID: 309 RVA: 0x00006135 File Offset: 0x00004335
		[MethodImpl(256)]
		public static double Truncate(this double value)
		{
			return Math.Truncate(value);
		}

		// Token: 0x06000136 RID: 310 RVA: 0x0000613D File Offset: 0x0000433D
		[MethodImpl(256)]
		public static int TruncateToInt(this float value)
		{
			return Convert.ToInt32(Math.Truncate((double)value));
		}

		// Token: 0x06000137 RID: 311 RVA: 0x0000614B File Offset: 0x0000434B
		[MethodImpl(256)]
		public static int TruncateToInt(this double value)
		{
			return Convert.ToInt32(Math.Truncate(value));
		}

		// Token: 0x06000138 RID: 312 RVA: 0x00006158 File Offset: 0x00004358
		[MethodImpl(256)]
		public static int Sign(this int value)
		{
			return Math.Sign(value);
		}

		// Token: 0x06000139 RID: 313 RVA: 0x00006160 File Offset: 0x00004360
		[MethodImpl(256)]
		public static int Sign(this float value)
		{
			return Math.Sign(value);
		}

		// Token: 0x0600013A RID: 314 RVA: 0x00006168 File Offset: 0x00004368
		[MethodImpl(256)]
		public static int Sign(this double value)
		{
			return Math.Sign(value);
		}

		// Token: 0x0600013B RID: 315 RVA: 0x00006170 File Offset: 0x00004370
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

		// Token: 0x0600013C RID: 316 RVA: 0x0000617F File Offset: 0x0000437F
		[MethodImpl(256)]
		public static float Clamp(this float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00006189 File Offset: 0x00004389
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

		// Token: 0x0600013E RID: 318 RVA: 0x00006198 File Offset: 0x00004398
		[MethodImpl(256)]
		public static float Clamp01(this float value)
		{
			return Mathf.Clamp01(value);
		}

		// Token: 0x0600013F RID: 319 RVA: 0x000061A0 File Offset: 0x000043A0
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

		// Token: 0x06000140 RID: 320 RVA: 0x000061CF File Offset: 0x000043CF
		[MethodImpl(256)]
		public static int Square(this int value)
		{
			return value * value;
		}

		// Token: 0x06000141 RID: 321 RVA: 0x000061D4 File Offset: 0x000043D4
		[MethodImpl(256)]
		public static float Square(this float value)
		{
			return value * value;
		}

		// Token: 0x06000142 RID: 322 RVA: 0x000061D9 File Offset: 0x000043D9
		[MethodImpl(256)]
		public static double Square(this double value)
		{
			return value * value;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x000061DE File Offset: 0x000043DE
		[MethodImpl(256)]
		public static float Sqrt(this float value)
		{
			return Math.Sqrt((double)value).ToFloat();
		}

		// Token: 0x06000144 RID: 324 RVA: 0x000061EC File Offset: 0x000043EC
		[MethodImpl(256)]
		public static double Sqrt(this double value)
		{
			return Math.Sqrt(value);
		}

		// Token: 0x06000145 RID: 325 RVA: 0x000061F4 File Offset: 0x000043F4
		[MethodImpl(256)]
		public static float Hypot(float a, float b)
		{
			return (a * a + b * b).Sqrt();
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00006202 File Offset: 0x00004402
		[MethodImpl(256)]
		public static double Hypot(double a, double b)
		{
			return (a * a + b * b).Sqrt();
		}

		// Token: 0x06000147 RID: 327 RVA: 0x00006210 File Offset: 0x00004410
		[MethodImpl(256)]
		public static float ToRadian(this float value)
		{
			return 0.017453292f * value;
		}

		// Token: 0x06000148 RID: 328 RVA: 0x00006219 File Offset: 0x00004419
		[MethodImpl(256)]
		public static float ToDegree(this float value)
		{
			return 57.29578f * value;
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00006222 File Offset: 0x00004422
		[MethodImpl(256)]
		public static float Lerp(this float t, float a, float b)
		{
			return Mathf.Lerp(a, b, t);
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000622C File Offset: 0x0000442C
		[MethodImpl(256)]
		public static Vector2 Lerp(this float t, Vector2 a, Vector2 b)
		{
			return Vector2.Lerp(a, b, t);
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00006236 File Offset: 0x00004436
		[MethodImpl(256)]
		public static Vector3 Lerp(this float t, Vector3 a, Vector3 b)
		{
			return Vector3.Lerp(a, b, t);
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00006240 File Offset: 0x00004440
		[MethodImpl(256)]
		public static Vector4 Lerp(this float t, Vector4 a, Vector4 b)
		{
			return Vector4.Lerp(a, b, t);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x0000624A File Offset: 0x0000444A
		[MethodImpl(256)]
		public static Quaternion Lerp(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.Lerp(a, b, t);
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00006254 File Offset: 0x00004454
		[MethodImpl(256)]
		public static Quaternion Slerp(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.Slerp(a, b, t);
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000625E File Offset: 0x0000445E
		[MethodImpl(256)]
		public static float LerpUnclamped(this float t, float a, float b)
		{
			return Mathf.LerpUnclamped(a, b, t);
		}

		// Token: 0x06000150 RID: 336 RVA: 0x00006268 File Offset: 0x00004468
		[MethodImpl(256)]
		public static Vector2 LerpUnclamped(this float t, Vector2 a, Vector2 b)
		{
			return Vector2.LerpUnclamped(a, b, t);
		}

		// Token: 0x06000151 RID: 337 RVA: 0x00006272 File Offset: 0x00004472
		[MethodImpl(256)]
		public static Vector3 LerpUnclamped(this float t, Vector3 a, Vector3 b)
		{
			return Vector3.LerpUnclamped(a, b, t);
		}

		// Token: 0x06000152 RID: 338 RVA: 0x0000627C File Offset: 0x0000447C
		[MethodImpl(256)]
		public static Vector4 LerpUnclamped(this float t, Vector4 a, Vector4 b)
		{
			return Vector4.LerpUnclamped(a, b, t);
		}

		// Token: 0x06000153 RID: 339 RVA: 0x00006286 File Offset: 0x00004486
		[MethodImpl(256)]
		public static Quaternion LerpUnclamped(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.LerpUnclamped(a, b, t);
		}

		// Token: 0x06000154 RID: 340 RVA: 0x00006290 File Offset: 0x00004490
		[MethodImpl(256)]
		public static Quaternion SlerpUnclamped(this float t, Quaternion a, Quaternion b)
		{
			return Quaternion.SlerpUnclamped(a, b, t);
		}

		// Token: 0x06000155 RID: 341 RVA: 0x0000629A File Offset: 0x0000449A
		[MethodImpl(256)]
		public static float InverseLerp(this float value, float a, float b)
		{
			return Mathf.InverseLerp(a, b, value);
		}

		// Token: 0x06000156 RID: 342 RVA: 0x000062A4 File Offset: 0x000044A4
		[MethodImpl(256)]
		public static float LerpAngle(this float t, float a, float b)
		{
			return Mathf.LerpAngle(a, b, t);
		}

		// Token: 0x06000157 RID: 343 RVA: 0x000062AE File Offset: 0x000044AE
		[MethodImpl(256)]
		[return: TupleElementNames(new string[] { "result", "remainder" })]
		public static ValueTuple<int, int> DivRem(this int a, int b)
		{
			return new ValueTuple<int, int>(a / b, a % b);
		}

		// Token: 0x06000158 RID: 344 RVA: 0x000062BB File Offset: 0x000044BB
		[MethodImpl(256)]
		public static void Deconstruct(this Vector2 v, out float x, out float y)
		{
			x = v.x;
			y = v.y;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x000062CD File Offset: 0x000044CD
		[MethodImpl(256)]
		public static void Deconstruct(this Vector2Int v, out int x, out int y)
		{
			x = v.x;
			y = v.y;
		}

		// Token: 0x0600015A RID: 346 RVA: 0x000062E1 File Offset: 0x000044E1
		[MethodImpl(256)]
		public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x000062FB File Offset: 0x000044FB
		[MethodImpl(256)]
		public static void Deconstruct(this Vector3Int v, out int x, out int y, out int z)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00006318 File Offset: 0x00004518
		[MethodImpl(256)]
		public static void Deconstruct(this Vector4 v, out float x, out float y, out float z, out float w)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			w = v.w;
		}

		// Token: 0x0600015D RID: 349 RVA: 0x0000633B File Offset: 0x0000453B
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
