using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LBoL.Core.Helpers
{
	// Token: 0x02000112 RID: 274
	public static class StreamingAssetsHelper
	{
		// Token: 0x060009E7 RID: 2535 RVA: 0x0001C42B File Offset: 0x0001A62B
		private static string ToAbsolutePath(string relativePath)
		{
			return Path.Combine(Application.streamingAssetsPath, relativePath);
		}

		// Token: 0x060009E8 RID: 2536 RVA: 0x0001C438 File Offset: 0x0001A638
		public static async UniTask<string> ReadAllTextAsync(string relativePath)
		{
			return await File.ReadAllTextAsync(StreamingAssetsHelper.ToAbsolutePath(relativePath), default(CancellationToken));
		}

		// Token: 0x060009E9 RID: 2537 RVA: 0x0001C47C File Offset: 0x0001A67C
		public static async UniTask<byte[]> ReadAllBytesAsync(string relativePath)
		{
			return await File.ReadAllBytesAsync(StreamingAssetsHelper.ToAbsolutePath(relativePath), default(CancellationToken));
		}
	}
}
