using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class BulletConfig
	{
		private BulletConfig(string Name, string Widget, string Launch, string LaunchSfx, string HitBody, string HitBodySfx, string HitShield, string HitShieldSfx, string HitBlock, string HitBlockSfx, string Graze, string GrazeSfx)
		{
			this.Name = Name;
			this.Widget = Widget;
			this.Launch = Launch;
			this.LaunchSfx = LaunchSfx;
			this.HitBody = HitBody;
			this.HitBodySfx = HitBodySfx;
			this.HitShield = HitShield;
			this.HitShieldSfx = HitShieldSfx;
			this.HitBlock = HitBlock;
			this.HitBlockSfx = HitBlockSfx;
			this.Graze = Graze;
			this.GrazeSfx = GrazeSfx;
		}
		public string Name { get; private set; }
		public string Widget { get; private set; }
		public string Launch { get; private set; }
		public string LaunchSfx { get; private set; }
		public string HitBody { get; private set; }
		public string HitBodySfx { get; private set; }
		public string HitShield { get; private set; }
		public string HitShieldSfx { get; private set; }
		public string HitBlock { get; private set; }
		public string HitBlockSfx { get; private set; }
		public string Graze { get; private set; }
		public string GrazeSfx { get; private set; }
		public static IReadOnlyList<BulletConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<BulletConfig>(BulletConfig._data);
		}
		public static BulletConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			BulletConfig bulletConfig;
			return (!BulletConfig._NameTable.TryGetValue(Name, out bulletConfig)) ? null : bulletConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{BulletConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Widget=",
				ConfigDataManager.System_String.ToString(this.Widget),
				", Launch=",
				ConfigDataManager.System_String.ToString(this.Launch),
				", LaunchSfx=",
				ConfigDataManager.System_String.ToString(this.LaunchSfx),
				", HitBody=",
				ConfigDataManager.System_String.ToString(this.HitBody),
				", HitBodySfx=",
				ConfigDataManager.System_String.ToString(this.HitBodySfx),
				", HitShield=",
				ConfigDataManager.System_String.ToString(this.HitShield),
				", HitShieldSfx=",
				ConfigDataManager.System_String.ToString(this.HitShieldSfx),
				", HitBlock=",
				ConfigDataManager.System_String.ToString(this.HitBlock),
				", HitBlockSfx=",
				ConfigDataManager.System_String.ToString(this.HitBlockSfx),
				", Graze=",
				ConfigDataManager.System_String.ToString(this.Graze),
				", GrazeSfx=",
				ConfigDataManager.System_String.ToString(this.GrazeSfx),
				"}"
			});
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				BulletConfig[] array = new BulletConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new BulletConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				BulletConfig._data = array;
				BulletConfig._NameTable = Enumerable.ToDictionary<BulletConfig, string>(BulletConfig._data, (BulletConfig elem) => elem.Name);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/BulletConfig");
			if (textAsset != null)
			{
				try
				{
					BulletConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load BulletConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'BulletConfig', please reimport config data");
			}
		}
		private static BulletConfig[] _data = Array.Empty<BulletConfig>();
		private static Dictionary<string, BulletConfig> _NameTable = Enumerable.ToDictionary<BulletConfig, string>(BulletConfig._data, (BulletConfig elem) => elem.Name);
	}
}
