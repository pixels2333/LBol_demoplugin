using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
namespace LBoL.Core
{
	public static class CrossPlatformHelper
	{
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(2)]
		private static extern bool GetWindowInfo(IntPtr hwnd, ref CrossPlatformHelper.WINDOWINFO pwi);
		[DllImport("user32.dll")]
		[return: MarshalAs(2)]
		private static extern bool EnumWindows(CrossPlatformHelper.EnumWindowsProc lpEnumFunc, IntPtr lParam);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
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
		[DllImport("user32.dll", CharSet = 3, EntryPoint = "SetWindowTextW", SetLastError = true)]
		[return: MarshalAs(2)]
		private static extern bool SetWindowText(IntPtr hwnd, [MarshalAs(21)] string lpString);
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
		private static readonly int ProcessId = Process.GetCurrentProcess().Id;
		[Flags]
		private enum WindowStyles : uint
		{
			WS_BORDER = 8388608U,
			WS_CAPTION = 12582912U,
			WS_CHILD = 1073741824U,
			WS_CLIPCHILDREN = 33554432U,
			WS_CLIPSIBLINGS = 67108864U,
			WS_DISABLED = 134217728U,
			WS_DLGFRAME = 4194304U,
			WS_GROUP = 131072U,
			WS_HSCROLL = 1048576U,
			WS_MAXIMIZE = 16777216U,
			WS_MAXIMIZEBOX = 65536U,
			WS_MINIMIZE = 536870912U,
			WS_MINIMIZEBOX = 131072U,
			WS_OVERLAPPED = 0U,
			WS_OVERLAPPEDWINDOW = 13565952U,
			WS_POPUP = 2147483648U,
			WS_POPUPWINDOW = 2156396544U,
			WS_SIZEFRAME = 262144U,
			WS_SYSMENU = 524288U,
			WS_TABSTOP = 65536U,
			WS_VISIBLE = 268435456U,
			WS_VSCROLL = 2097152U
		}
		[Flags]
		private enum WindowStylesEx : uint
		{
			WS_EX_ACCEPTFILES = 16U,
			WS_EX_APPWINDOW = 262144U,
			WS_EX_CLIENTEDGE = 512U,
			WS_EX_COMPOSITED = 33554432U,
			WS_EX_CONTEXTHELP = 1024U,
			WS_EX_CONTROLPARENT = 65536U,
			WS_EX_DLGMODALFRAME = 1U,
			WS_EX_LAYERED = 524288U,
			WS_EX_LAYOUTRTL = 4194304U,
			WS_EX_LEFT = 0U,
			WS_EX_LEFTSCROLLBAR = 16384U,
			WS_EX_LTRREADING = 0U,
			WS_EX_MDICHILD = 64U,
			WS_EX_NOACTIVATE = 134217728U,
			WS_EX_NOINHERITLAYOUT = 1048576U,
			WS_EX_NOPARENTNOTIFY = 4U,
			WS_EX_NOREDIRECTIONBITMAP = 2097152U,
			WS_EX_OVERLAPPEDWINDOW = 768U,
			WS_EX_PALETTEWINDOW = 392U,
			WS_EX_RIGHT = 4096U,
			WS_EX_RIGHTSCROLLBAR = 0U,
			WS_EX_RTLREADING = 8192U,
			WS_EX_STATICEDGE = 131072U,
			WS_EX_TOOLWINDOW = 128U,
			WS_EX_TOPMOST = 8U,
			WS_EX_TRANSPARENT = 32U,
			WS_EX_WINDOWEDGE = 256U
		}
		private struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}
		private struct WINDOWINFO
		{
			public uint cbSize;
			public CrossPlatformHelper.RECT rcWindow;
			public CrossPlatformHelper.RECT rcClient;
			public CrossPlatformHelper.WindowStyles dwStyle;
			public CrossPlatformHelper.WindowStylesEx dwExStyle;
			public uint dwWindowStatus;
			public uint cxWindowBorders;
			public uint cyWindowBorders;
			public ushort atomWindowType;
			public ushort wCreatorVersion;
		}
		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
	}
}
