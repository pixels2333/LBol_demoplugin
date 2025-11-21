using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000006 RID: 6
	public sealed class SpineEventConfig
	{
		// Token: 0x06000057 RID: 87 RVA: 0x00002E0F File Offset: 0x0000100F
		private SpineEventConfig(string Name, string Sfx, string Effect)
		{
			this.Name = Name;
			this.Sfx = Sfx;
			this.Effect = Effect;
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000058 RID: 88 RVA: 0x00002E2C File Offset: 0x0000102C
		// (set) Token: 0x06000059 RID: 89 RVA: 0x00002E34 File Offset: 0x00001034
		public string Name { get; private set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600005A RID: 90 RVA: 0x00002E3D File Offset: 0x0000103D
		// (set) Token: 0x0600005B RID: 91 RVA: 0x00002E45 File Offset: 0x00001045
		public string Sfx { get; private set; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600005C RID: 92 RVA: 0x00002E4E File Offset: 0x0000104E
		// (set) Token: 0x0600005D RID: 93 RVA: 0x00002E56 File Offset: 0x00001056
		public string Effect { get; private set; }

		// Token: 0x0600005E RID: 94 RVA: 0x00002E5F File Offset: 0x0000105F
		public static IReadOnlyList<SpineEventConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SpineEventConfig>(SpineEventConfig._data);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00002E70 File Offset: 0x00001070
		public static SpineEventConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			SpineEventConfig spineEventConfig;
			return (!SpineEventConfig._NameTable.TryGetValue(Name, out spineEventConfig)) ? null : spineEventConfig;
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002E9C File Offset: 0x0000109C
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{SpineEventConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Sfx=",
				ConfigDataManager.System_String.ToString(this.Sfx),
				", Effect=",
				ConfigDataManager.System_String.ToString(this.Effect),
				"}"
			});
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00002F10 File Offset: 0x00001110
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SpineEventConfig[] array = new SpineEventConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SpineEventConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				SpineEventConfig._data = array;
				SpineEventConfig._NameTable = Enumerable.ToDictionary<SpineEventConfig, string>(SpineEventConfig._data, (SpineEventConfig elem) => elem.Name);
			}
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002FC8 File Offset: 0x000011C8
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SpineEventConfig");
			if (textAsset != null)
			{
				try
				{
					SpineEventConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SpineEventConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SpineEventConfig', please reimport config data");
			}
		}

		// Token: 0x04000029 RID: 41
		private static SpineEventConfig[] _data = Array.Empty<SpineEventConfig>();

		// Token: 0x0400002A RID: 42
		private static Dictionary<string, SpineEventConfig> _NameTable = Enumerable.ToDictionary<SpineEventConfig, string>(SpineEventConfig._data, (SpineEventConfig elem) => elem.Name);
	}
}
