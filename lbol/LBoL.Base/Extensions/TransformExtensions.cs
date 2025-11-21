using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LBoL.Base.Extensions
{
	// Token: 0x02000023 RID: 35
	public static class TransformExtensions
	{
		// Token: 0x06000181 RID: 385 RVA: 0x000068B3 File Offset: 0x00004AB3
		[MethodImpl(256)]
		private static ArgumentNullException ArgumentNull(string name)
		{
			return new ArgumentNullException(name);
		}

		// Token: 0x06000182 RID: 386 RVA: 0x000068BB File Offset: 0x00004ABB
		[MethodImpl(256)]
		public static void SetLocalX(this Transform transform, float x)
		{
			transform.localPosition = transform.localPosition.WithX(x);
		}

		// Token: 0x06000183 RID: 387 RVA: 0x000068CF File Offset: 0x00004ACF
		[MethodImpl(256)]
		public static void SetLocalY(this Transform transform, float y)
		{
			transform.localPosition = transform.localPosition.WithY(y);
		}

		// Token: 0x06000184 RID: 388 RVA: 0x000068E3 File Offset: 0x00004AE3
		[MethodImpl(256)]
		public static void SetLocalZ(this Transform transform, float z)
		{
			transform.localPosition = transform.localPosition.WithZ(z);
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000068F8 File Offset: 0x00004AF8
		[MethodImpl(256)]
		public static void DestroyChildren(this Transform transform)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Object.Destroy(transform.GetChild(i).gameObject);
			}
		}
	}
}
