using System;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace LBoL.Base.Extensions
{
	public static class TransformExtensions
	{
		[MethodImpl(256)]
		private static ArgumentNullException ArgumentNull(string name)
		{
			return new ArgumentNullException(name);
		}
		[MethodImpl(256)]
		public static void SetLocalX(this Transform transform, float x)
		{
			transform.localPosition = transform.localPosition.WithX(x);
		}
		[MethodImpl(256)]
		public static void SetLocalY(this Transform transform, float y)
		{
			transform.localPosition = transform.localPosition.WithY(y);
		}
		[MethodImpl(256)]
		public static void SetLocalZ(this Transform transform, float z)
		{
			transform.localPosition = transform.localPosition.WithZ(z);
		}
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
