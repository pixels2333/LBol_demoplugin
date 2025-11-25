using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace LBoL.Core.Helpers
{
	public static class StreamingAssetsHelper
	{
		private static string ToAbsolutePath(string relativePath)
		{
			return Path.Combine(Application.streamingAssetsPath, relativePath);
		}
		public static async UniTask<string> ReadAllTextAsync(string relativePath)
		{
			return await File.ReadAllTextAsync(StreamingAssetsHelper.ToAbsolutePath(relativePath), default(CancellationToken));
		}
		public static async UniTask<byte[]> ReadAllBytesAsync(string relativePath)
		{
			return await File.ReadAllBytesAsync(StreamingAssetsHelper.ToAbsolutePath(relativePath), default(CancellationToken));
		}
	}
}
