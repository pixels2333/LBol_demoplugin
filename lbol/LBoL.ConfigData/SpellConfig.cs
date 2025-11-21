using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200000F RID: 15
	public sealed class SpellConfig
	{
		// Token: 0x06000212 RID: 530 RVA: 0x00007747 File Offset: 0x00005947
		private SpellConfig(string Id, string Resource)
		{
			this.Id = Id;
			this.Resource = Resource;
		}

		// Token: 0x170000BC RID: 188
		// (get) Token: 0x06000213 RID: 531 RVA: 0x0000775D File Offset: 0x0000595D
		// (set) Token: 0x06000214 RID: 532 RVA: 0x00007765 File Offset: 0x00005965
		public string Id { get; private set; }

		// Token: 0x170000BD RID: 189
		// (get) Token: 0x06000215 RID: 533 RVA: 0x0000776E File Offset: 0x0000596E
		// (set) Token: 0x06000216 RID: 534 RVA: 0x00007776 File Offset: 0x00005976
		public string Resource { get; private set; }

		// Token: 0x06000217 RID: 535 RVA: 0x0000777F File Offset: 0x0000597F
		public static IReadOnlyList<SpellConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<SpellConfig>(SpellConfig._data);
		}

		// Token: 0x06000218 RID: 536 RVA: 0x00007790 File Offset: 0x00005990
		public static SpellConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			SpellConfig spellConfig;
			return (!SpellConfig._IdTable.TryGetValue(Id, out spellConfig)) ? null : spellConfig;
		}

		// Token: 0x06000219 RID: 537 RVA: 0x000077BC File Offset: 0x000059BC
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{SpellConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", Resource=",
				ConfigDataManager.System_String.ToString(this.Resource),
				"}"
			});
		}

		// Token: 0x0600021A RID: 538 RVA: 0x00007814 File Offset: 0x00005A14
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				SpellConfig[] array = new SpellConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new SpellConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader));
				}
				SpellConfig._data = array;
				SpellConfig._IdTable = Enumerable.ToDictionary<SpellConfig, string>(SpellConfig._data, (SpellConfig elem) => elem.Id);
			}
		}

		// Token: 0x0600021B RID: 539 RVA: 0x000078C0 File Offset: 0x00005AC0
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/SpellConfig");
			if (textAsset != null)
			{
				try
				{
					SpellConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load SpellConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'SpellConfig', please reimport config data");
			}
		}

		// Token: 0x0400010B RID: 267
		private static SpellConfig[] _data = Array.Empty<SpellConfig>();

		// Token: 0x0400010C RID: 268
		private static Dictionary<string, SpellConfig> _IdTable = Enumerable.ToDictionary<SpellConfig, string>(SpellConfig._data, (SpellConfig elem) => elem.Id);
	}
}
