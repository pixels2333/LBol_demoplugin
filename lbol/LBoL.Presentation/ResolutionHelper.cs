using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core;
using UnityEngine;

namespace LBoL.Presentation
{
	// Token: 0x0200000F RID: 15
	public static class ResolutionHelper
	{
		// Token: 0x06000166 RID: 358 RVA: 0x000076B3 File Offset: 0x000058B3
		public static bool IsValidAspect(Vector2Int size)
		{
			return ResolutionHelper.Resolutions.Contains(size);
		}

		// Token: 0x06000167 RID: 359 RVA: 0x000076C0 File Offset: 0x000058C0
		public static IReadOnlyList<Vector2Int> GetAvailableResolutions()
		{
			DisplayInfo mainWindowDisplayInfo = Screen.mainWindowDisplayInfo;
			List<Vector2Int> list = new List<Vector2Int>();
			foreach (Vector2Int vector2Int in ResolutionHelper.Resolutions)
			{
				if (vector2Int.x <= mainWindowDisplayInfo.width && vector2Int.y <= mainWindowDisplayInfo.height)
				{
					list.Add(vector2Int);
				}
			}
			return list.AsReadOnly();
		}

		// Token: 0x06000168 RID: 360 RVA: 0x00007744 File Offset: 0x00005944
		public static IReadOnlyList<FrameSetting> GetAvailableFrameSettings()
		{
			DisplayInfo mainWindowDisplayInfo = Screen.mainWindowDisplayInfo;
			List<FrameSetting> list = new List<FrameSetting>();
			for (int i = 1; i < 4; i++)
			{
				list.Add(new FrameSetting(i, (mainWindowDisplayInfo.refreshRate.value / (double)i).RoundToInt()));
			}
			list.Add(new FrameSetting(0, -1));
			int num = mainWindowDisplayInfo.refreshRate.value.RoundToInt();
			list.Add(new FrameSetting(0, num));
			foreach (int num2 in ResolutionHelper.FrameRates)
			{
				if (num2 < num)
				{
					list.Add(new FrameSetting(0, num2));
				}
			}
			return list.AsReadOnly();
		}

		// Token: 0x06000169 RID: 361 RVA: 0x00007810 File Offset: 0x00005A10
		public static string LocalizeTextForFrameSetting(FrameSetting frameSetting)
		{
			if (frameSetting.VsyncCount == 0 && frameSetting.FrameRate == -1)
			{
				return "Setting.NoLimit".Localize(true);
			}
			if (frameSetting.IsVsync)
			{
				return "Setting.Vsync".LocalizeFormat(new object[] { frameSetting.FrameRate });
			}
			return "Setting.NonVsync".LocalizeFormat(new object[] { frameSetting.FrameRate });
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00007880 File Offset: 0x00005A80
		// Note: this type is marked as 'beforefieldinit'.
		static ResolutionHelper()
		{
			List<Vector2Int> list = new List<Vector2Int>();
			list.Add(new Vector2Int(640, 360));
			list.Add(new Vector2Int(960, 540));
			list.Add(new Vector2Int(1280, 720));
			list.Add(new Vector2Int(1366, 768));
			list.Add(new Vector2Int(1600, 900));
			list.Add(new Vector2Int(1920, 1080));
			list.Add(new Vector2Int(2560, 1440));
			list.Add(new Vector2Int(3200, 1800));
			list.Add(new Vector2Int(3840, 2160));
			list.Add(new Vector2Int(5120, 2880));
			list.Add(new Vector2Int(7680, 4320));
			ResolutionHelper.Resolutions = list;
			List<int> list2 = new List<int>();
			list2.Add(240);
			list2.Add(180);
			list2.Add(120);
			list2.Add(60);
			list2.Add(30);
			ResolutionHelper.FrameRates = list2;
		}

		// Token: 0x04000064 RID: 100
		private static readonly List<Vector2Int> Resolutions;

		// Token: 0x04000065 RID: 101
		private static readonly List<int> FrameRates;
	}
}
