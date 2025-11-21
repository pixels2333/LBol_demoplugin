using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000077 RID: 119
	public static class Utils
	{
		// Token: 0x0600053F RID: 1343 RVA: 0x00011944 File Offset: 0x0000FB44
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

		// Token: 0x06000540 RID: 1344 RVA: 0x00011990 File Offset: 0x0000FB90
		public static int[] GenerateRandomIndices(int count, int limit, RandomGen rng)
		{
			return Utils.GenerateRandomIndices(count, limit, new Func<int, int, int>(rng.NextInt));
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x000119A5 File Offset: 0x0000FBA5
		public static GameObject CreateGameObject(Transform parent, string name)
		{
			GameObject gameObject = ((parent is RectTransform) ? new GameObject(name, new Type[] { typeof(RectTransform) }) : new GameObject(name));
			gameObject.transform.SetParent(parent, false);
			return gameObject;
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x000119E0 File Offset: 0x0000FBE0
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

		// Token: 0x06000543 RID: 1347 RVA: 0x00011A24 File Offset: 0x0000FC24
		public static bool TryParseIso8601Timestamp(string timestamp, out DateTime result)
		{
			return DateTime.TryParse(timestamp, ref result);
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00011A30 File Offset: 0x0000FC30
		public static string SecondsToHHMMSS(int seconds)
		{
			int num2;
			int num3;
			int num = Math.DivRem(Math.DivRem(seconds, 60, ref num2), 60, ref num3);
			return string.Format("{0:00}:{1:00}:{2:00}", num, num3, num2);
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x00011A70 File Offset: 0x0000FC70
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
