using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000008 RID: 8
	public static class CrossPlatformHelper
	{
		// Token: 0x0600000E RID: 14
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(2)]
		private static extern bool GetWindowInfo(IntPtr hwnd, ref CrossPlatformHelper.WINDOWINFO pwi);

		// Token: 0x0600000F RID: 15
		[DllImport("user32.dll")]
		[return: MarshalAs(2)]
		private static extern bool EnumWindows(CrossPlatformHelper.EnumWindowsProc lpEnumFunc, IntPtr lParam);

		// Token: 0x06000010 RID: 16
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		// Token: 0x06000011 RID: 17 RVA: 0x000021C8 File Offset: 0x000003C8
		private static bool EnumProc(IntPtr hWnd, IntPtr lParam)
		{
			uint num;
			CrossPlatformHelper.GetWindowThreadProcessId(hWnd, out num);
			if ((ulong)num == (ulong)((long)CrossPlatformHelper.ProcessId))
			{
				CrossPlatformHelper.WINDOWINFO windowinfo = default(CrossPlatformHelper.WINDOWINFO);
				windowinfo.cbSize = (uint)Marshal.SizeOf<CrossPlatformHelper.WINDOWINFO>(windowinfo);
				CrossPlatformHelper.GetWindowInfo(hWnd, ref windowinfo);
				if (windowinfo.dwStyle.HasFlag(CrossPlatformHelper.WindowStyles.WS_VISIBLE))
				{
					((List<IntPtr>)GCHandle.FromIntPtr(lParam).Target).Add(hWnd);
				}
			}
			return true;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000223C File Offset: 0x0000043C
		private static IntPtr GetMainWindow()
		{
			List<IntPtr> list = new List<IntPtr>();
			GCHandle gchandle = GCHandle.Alloc(list, 3);
			CrossPlatformHelper.EnumWindows(new CrossPlatformHelper.EnumWindowsProc(CrossPlatformHelper.EnumProc), GCHandle.ToIntPtr(gchandle));
			gchandle.Free();
			if (list.Count > 1)
			{
				Debug.LogError("Multiple Window Exists");
			}
			return Enumerable.FirstOrDefault<IntPtr>(list);
		}

		// Token: 0x06000013 RID: 19
		[DllImport("user32.dll", CharSet = 3, EntryPoint = "SetWindowTextW", SetLastError = true)]
		[return: MarshalAs(2)]
		private static extern bool SetWindowText(IntPtr hwnd, [MarshalAs(21)] string lpString);

		// Token: 0x06000014 RID: 20 RVA: 0x00002290 File Offset: 0x00000490
		public static void SetWindowTitle(string title)
		{
			IntPtr mainWindow = CrossPlatformHelper.GetMainWindow();
			if (mainWindow != IntPtr.Zero)
			{
				if (!CrossPlatformHelper.SetWindowText(mainWindow, title))
				{
					Debug.LogError(string.Format("Failed to set window title: last error = {0}", Marshal.GetLastWin32Error()));
					return;
				}
			}
			else
			{
				Debug.LogError(string.Format("Failed to get active window: last error = {0}", Marshal.GetLastWin32Error()));
			}
		}

		// Token: 0x04000050 RID: 80
		private static readonly int ProcessId = Process.GetCurrentProcess().Id;

		// Token: 0x020001C2 RID: 450
		[Flags]
		private enum WindowStyles : uint
		{
			// Token: 0x040006DE RID: 1758
			WS_BORDER = 8388608U,
			// Token: 0x040006DF RID: 1759
			WS_CAPTION = 12582912U,
			// Token: 0x040006E0 RID: 1760
			WS_CHILD = 1073741824U,
			// Token: 0x040006E1 RID: 1761
			WS_CLIPCHILDREN = 33554432U,
			// Token: 0x040006E2 RID: 1762
			WS_CLIPSIBLINGS = 67108864U,
			// Token: 0x040006E3 RID: 1763
			WS_DISABLED = 134217728U,
			// Token: 0x040006E4 RID: 1764
			WS_DLGFRAME = 4194304U,
			// Token: 0x040006E5 RID: 1765
			WS_GROUP = 131072U,
			// Token: 0x040006E6 RID: 1766
			WS_HSCROLL = 1048576U,
			// Token: 0x040006E7 RID: 1767
			WS_MAXIMIZE = 16777216U,
			// Token: 0x040006E8 RID: 1768
			WS_MAXIMIZEBOX = 65536U,
			// Token: 0x040006E9 RID: 1769
			WS_MINIMIZE = 536870912U,
			// Token: 0x040006EA RID: 1770
			WS_MINIMIZEBOX = 131072U,
			// Token: 0x040006EB RID: 1771
			WS_OVERLAPPED = 0U,
			// Token: 0x040006EC RID: 1772
			WS_OVERLAPPEDWINDOW = 13565952U,
			// Token: 0x040006ED RID: 1773
			WS_POPUP = 2147483648U,
			// Token: 0x040006EE RID: 1774
			WS_POPUPWINDOW = 2156396544U,
			// Token: 0x040006EF RID: 1775
			WS_SIZEFRAME = 262144U,
			// Token: 0x040006F0 RID: 1776
			WS_SYSMENU = 524288U,
			// Token: 0x040006F1 RID: 1777
			WS_TABSTOP = 65536U,
			// Token: 0x040006F2 RID: 1778
			WS_VISIBLE = 268435456U,
			// Token: 0x040006F3 RID: 1779
			WS_VSCROLL = 2097152U
		}

		// Token: 0x020001C3 RID: 451
		[Flags]
		private enum WindowStylesEx : uint
		{
			// Token: 0x040006F5 RID: 1781
			WS_EX_ACCEPTFILES = 16U,
			// Token: 0x040006F6 RID: 1782
			WS_EX_APPWINDOW = 262144U,
			// Token: 0x040006F7 RID: 1783
			WS_EX_CLIENTEDGE = 512U,
			// Token: 0x040006F8 RID: 1784
			WS_EX_COMPOSITED = 33554432U,
			// Token: 0x040006F9 RID: 1785
			WS_EX_CONTEXTHELP = 1024U,
			// Token: 0x040006FA RID: 1786
			WS_EX_CONTROLPARENT = 65536U,
			// Token: 0x040006FB RID: 1787
			WS_EX_DLGMODALFRAME = 1U,
			// Token: 0x040006FC RID: 1788
			WS_EX_LAYERED = 524288U,
			// Token: 0x040006FD RID: 1789
			WS_EX_LAYOUTRTL = 4194304U,
			// Token: 0x040006FE RID: 1790
			WS_EX_LEFT = 0U,
			// Token: 0x040006FF RID: 1791
			WS_EX_LEFTSCROLLBAR = 16384U,
			// Token: 0x04000700 RID: 1792
			WS_EX_LTRREADING = 0U,
			// Token: 0x04000701 RID: 1793
			WS_EX_MDICHILD = 64U,
			// Token: 0x04000702 RID: 1794
			WS_EX_NOACTIVATE = 134217728U,
			// Token: 0x04000703 RID: 1795
			WS_EX_NOINHERITLAYOUT = 1048576U,
			// Token: 0x04000704 RID: 1796
			WS_EX_NOPARENTNOTIFY = 4U,
			// Token: 0x04000705 RID: 1797
			WS_EX_NOREDIRECTIONBITMAP = 2097152U,
			// Token: 0x04000706 RID: 1798
			WS_EX_OVERLAPPEDWINDOW = 768U,
			// Token: 0x04000707 RID: 1799
			WS_EX_PALETTEWINDOW = 392U,
			// Token: 0x04000708 RID: 1800
			WS_EX_RIGHT = 4096U,
			// Token: 0x04000709 RID: 1801
			WS_EX_RIGHTSCROLLBAR = 0U,
			// Token: 0x0400070A RID: 1802
			WS_EX_RTLREADING = 8192U,
			// Token: 0x0400070B RID: 1803
			WS_EX_STATICEDGE = 131072U,
			// Token: 0x0400070C RID: 1804
			WS_EX_TOOLWINDOW = 128U,
			// Token: 0x0400070D RID: 1805
			WS_EX_TOPMOST = 8U,
			// Token: 0x0400070E RID: 1806
			WS_EX_TRANSPARENT = 32U,
			// Token: 0x0400070F RID: 1807
			WS_EX_WINDOWEDGE = 256U
		}

		// Token: 0x020001C4 RID: 452
		private struct RECT
		{
			// Token: 0x04000710 RID: 1808
			public int Left;

			// Token: 0x04000711 RID: 1809
			public int Top;

			// Token: 0x04000712 RID: 1810
			public int Right;

			// Token: 0x04000713 RID: 1811
			public int Bottom;
		}

		// Token: 0x020001C5 RID: 453
		private struct WINDOWINFO
		{
			// Token: 0x04000714 RID: 1812
			public uint cbSize;

			// Token: 0x04000715 RID: 1813
			public CrossPlatformHelper.RECT rcWindow;

			// Token: 0x04000716 RID: 1814
			public CrossPlatformHelper.RECT rcClient;

			// Token: 0x04000717 RID: 1815
			public CrossPlatformHelper.WindowStyles dwStyle;

			// Token: 0x04000718 RID: 1816
			public CrossPlatformHelper.WindowStylesEx dwExStyle;

			// Token: 0x04000719 RID: 1817
			public uint dwWindowStatus;

			// Token: 0x0400071A RID: 1818
			public uint cxWindowBorders;

			// Token: 0x0400071B RID: 1819
			public uint cyWindowBorders;

			// Token: 0x0400071C RID: 1820
			public ushort atomWindowType;

			// Token: 0x0400071D RID: 1821
			public ushort wCreatorVersion;
		}

		// Token: 0x020001C6 RID: 454
		// (Invoke) Token: 0x06000FE9 RID: 4073
		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	}
}
