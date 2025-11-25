using System;
using UnityEngine;
namespace LBoL.Core
{
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
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
		static Singleton()
		{
			Application.quitting += delegate
			{
				Singleton<T>._quitting = true;
			};
		}
		private void OnDestroy()
		{
			Singleton<T>._quitting = true;
		}
		private static bool _quitting;
		private static readonly Lazy<T> LazyInstance = new Lazy<T>(new Func<T>(Singleton<T>.Create));
	}
}
