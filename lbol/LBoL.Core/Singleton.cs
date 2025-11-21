using System;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x0200006D RID: 109
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		// Token: 0x06000484 RID: 1156 RVA: 0x0000FA4C File Offset: 0x0000DC4C
		private static T Create()
		{
			T t = (T)((object)Object.FindObjectOfType(typeof(T)));
			if (t != null)
			{
				return t;
			}
			GameObject gameObject = new GameObject("[" + typeof(T).Name + "] (Singleton)");
			t = gameObject.AddComponent<T>();
			Object.DontDestroyOnLoad(gameObject);
			return t;
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x06000485 RID: 1157 RVA: 0x0000FAB0 File Offset: 0x0000DCB0
		public static T Instance
		{
			get
			{
				if (!Singleton<T>._quitting)
				{
					return Singleton<T>.LazyInstance.Value;
				}
				return default(T);
			}
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x0000FAD8 File Offset: 0x0000DCD8
		static Singleton()
		{
			Application.quitting += delegate
			{
				Singleton<T>._quitting = true;
			};
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x0000FB05 File Offset: 0x0000DD05
		private void OnDestroy()
		{
			Singleton<T>._quitting = true;
		}

		// Token: 0x04000260 RID: 608
		private static bool _quitting;

		// Token: 0x04000261 RID: 609
		private static readonly Lazy<T> LazyInstance = new Lazy<T>(new Func<T>(Singleton<T>.Create));
	}
}
