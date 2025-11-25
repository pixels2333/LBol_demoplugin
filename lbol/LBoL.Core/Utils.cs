using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.Core
{
	public static class Utils
	{
		public static int[] GenerateRandomIndices(int count, int limit, Func<int, int, int> rand)
		{
			int[] array = Enumerable.ToArray<int>(Enumerable.Range(0, limit));
			int[] array2 = new int[count];
			int num = limit;
			for (int i = 0; i < count; i++)
			{
				num--;
				int num2 = rand.Invoke(0, num);
				array2[i] = array[num2];
				array[num2] = array[num];
			}
			return array2;
		}
		public static int[] GenerateRandomIndices(int count, int limit, RandomGen rng)
		{
			return Utils.GenerateRandomIndices(count, limit, new Func<int, int, int>(rng.NextInt));
		}
		public static GameObject CreateGameObject(Transform parent, string name)
		{
			GameObject gameObject = ((parent is RectTransform) ? new GameObject(name, new Type[] { typeof(RectTransform) }) : new GameObject(name));
			gameObject.transform.SetParent(parent, false);
			return gameObject;
		}
		public static string ToIso8601Timestamp(DateTime time)
		{
			DateTimeKind kind = time.Kind;
			DateTime dateTime;
			if (kind != 1)
			{
				if (kind != 2)
				{
					throw new ArgumentException("Cannot recognize time's kind");
				}
				dateTime = time.ToUniversalTime();
			}
			else
			{
				dateTime = time;
			}
			DateTime dateTime2 = dateTime;
			return dateTime2.ToString("yyyy-MM-ddTHH:mm:ssZ");
		}
		public static bool TryParseIso8601Timestamp(string timestamp, out DateTime result)
		{
			return DateTime.TryParse(timestamp, ref result);
		}
		public static string SecondsToHHMMSS(int seconds)
		{
			int num2;
			int num3;
			int num = Math.DivRem(Math.DivRem(seconds, 60, ref num2), 60, ref num3);
			return string.Format("{0:00}:{1:00}:{2:00}", num, num3, num2);
		}
		public static string GetScenePath(Transform transform)
		{
			if (!transform)
			{
				return "<Error: transform reference missing>";
			}
			List<string> list = new List<string>();
			for (;;)
			{
				list.Add(transform.name);
				Transform parent = transform.parent;
				if (parent == null)
				{
					break;
				}
				transform = parent;
			}
			return string.Join<string>('/', Enumerable.Reverse<string>(list));
		}
	}
}
