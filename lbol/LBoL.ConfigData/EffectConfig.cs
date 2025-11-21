using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000005 RID: 5
	public sealed class EffectConfig
	{
		// Token: 0x06000048 RID: 72 RVA: 0x00002BAB File Offset: 0x00000DAB
		private EffectConfig(string Name, string Path, float Life)
		{
			this.Name = Name;
			this.Path = Path;
			this.Life = Life;
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00002BC8 File Offset: 0x00000DC8
		// (set) Token: 0x0600004A RID: 74 RVA: 0x00002BD0 File Offset: 0x00000DD0
		public string Name { get; private set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00002BD9 File Offset: 0x00000DD9
		// (set) Token: 0x0600004C RID: 76 RVA: 0x00002BE1 File Offset: 0x00000DE1
		public string Path { get; private set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600004D RID: 77 RVA: 0x00002BEA File Offset: 0x00000DEA
		// (set) Token: 0x0600004E RID: 78 RVA: 0x00002BF2 File Offset: 0x00000DF2
		public float Life { get; private set; }

		// Token: 0x0600004F RID: 79 RVA: 0x00002BFB File Offset: 0x00000DFB
		public static IReadOnlyList<EffectConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<EffectConfig>(EffectConfig._data);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00002C0C File Offset: 0x00000E0C
		public static EffectConfig FromName(string Name)
		{
			ConfigDataManager.Initialize();
			EffectConfig effectConfig;
			return (!EffectConfig._NameTable.TryGetValue(Name, out effectConfig)) ? null : effectConfig;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00002C38 File Offset: 0x00000E38
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{EffectConfig Name=",
				ConfigDataManager.System_String.ToString(this.Name),
				", Path=",
				ConfigDataManager.System_String.ToString(this.Path),
				", Life=",
				ConfigDataManager.System_Single.ToString(this.Life),
				"}"
			});
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002CAC File Offset: 0x00000EAC
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				EffectConfig[] array = new EffectConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new EffectConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Single.ReadFrom(binaryReader));
				}
				EffectConfig._data = array;
				EffectConfig._NameTable = Enumerable.ToDictionary<EffectConfig, string>(EffectConfig._data, (EffectConfig elem) => elem.Name);
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00002D64 File Offset: 0x00000F64
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/EffectConfig");
			if (textAsset != null)
			{
				try
				{
					EffectConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load EffectConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'EffectConfig', please reimport config data");
			}
		}

		// Token: 0x04000023 RID: 35
		private static EffectConfig[] _data = Array.Empty<EffectConfig>();

		// Token: 0x04000024 RID: 36
		private static Dictionary<string, EffectConfig> _NameTable = Enumerable.ToDictionary<EffectConfig, string>(EffectConfig._data, (EffectConfig elem) => elem.Name);
	}
}
