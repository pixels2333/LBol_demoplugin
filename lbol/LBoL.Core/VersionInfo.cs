using System;
using UnityEngine;
using YamlDotNet.Serialization;
namespace LBoL.Core
{
	[Serializable]
	public sealed class VersionInfo
	{
		public string Revision { get; set; }
		public string Version { get; set; }
		public bool IsBeta { get; set; }
		public string BuildDateTime { get; set; }
		public static VersionInfo Current { get; } = new Deserializer().Deserialize<VersionInfo>(Resources.Load<TextAsset>("VersionInfo").text);
	}
}
