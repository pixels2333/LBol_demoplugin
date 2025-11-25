using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class LaserConfig
	{
		private LaserConfig(string Name, string Widget, string LaunchSfx, Vector2 Size, Vector2 Offset, int Start, string HitBody, string HitBodySfx, string HitShield, string HitShieldSfx, string HitBlock, string HitBlockSfx, string Graze, string GrazeSfx)
		{
			this.Name = Name;
			this.Widget = Widget;
			this.LaunchSfx = LaunchSfx;
			this.Size = Size;
			this.Offset = Offset;
			this.Start = Start;
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
		public string LaunchSfx { get; private set; }
		public Vector2 Size { get; private set; }
		public Vector2 Offset { get; private set; }
		public int Start { get; private set; }
		public string HitBody { get; private set; }
		public string HitBodySfx { get; private set; }
		public string HitShield { get; private set; }
		public string HitShieldSfx { get; private set; }
		public string HitBlock { get; private set; }
		public string HitBlockSfx { get; private set; }
		public string Graze { get; private set; }
		public string GrazeSfx { get; private set; }
		public static IReadOnlyList<LaserConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<LaserConfig>(LaserConfig._data);
		}
		public static LaserConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			LaserConfig laserConfig;
			return (!LaserConfig._NameTable.TryGetValue(Name, out laserConfig)) ? null : laserConfig;
		}
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{LaserConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Widget=",
				ConfigDataManager.System_String.ToString(this.Widget),
				", LaunchSfx=",
				ConfigDataManager.System_String.ToString(this.LaunchSfx),
				", Size=",
				ConfigDataManager.UnityEngine_Vector2.ToString(this.Size),
				", Offset=",
				ConfigDataManager.UnityEngine_Vector2.ToString(this.Offset),
				", Start=",
				ConfigDataManager.System_Int32.ToString(this.Start),
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
				LaserConfig[] array = new LaserConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new LaserConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.UnityEngine_Vector2.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				LaserConfig._data = array;
				LaserConfig._NameTable = Enumerable.ToDictionary<LaserConfig, string>(LaserConfig._data, (LaserConfig elem) => elem.Name);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/LaserConfig");
			if (textAsset != null)
			{
				try
				{
					LaserConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load LaserConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'LaserConfig', please reimport config data");
			}
		}
		private static LaserConfig[] _data = Array.Empty<LaserConfig>();
		private static Dictionary<string, LaserConfig> _NameTable = Enumerable.ToDictionary<LaserConfig, string>(LaserConfig._data, (LaserConfig elem) => elem.Name);
	}
}
