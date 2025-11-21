using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200001B RID: 27
	public sealed class BluePointConfig
	{
		// Token: 0x06000474 RID: 1140 RVA: 0x0000DBDB File Offset: 0x0000BDDB
		private BluePointConfig(string Id, int? BluePoint)
		{
			this.Id = Id;
			this.BluePoint = BluePoint;
		}

		// Token: 0x17000183 RID: 387
		// (get) Token: 0x06000475 RID: 1141 RVA: 0x0000DBF1 File Offset: 0x0000BDF1
		// (set) Token: 0x06000476 RID: 1142 RVA: 0x0000DBF9 File Offset: 0x0000BDF9
		public string Id { get; private set; }

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x06000477 RID: 1143 RVA: 0x0000DC02 File Offset: 0x0000BE02
		// (set) Token: 0x06000478 RID: 1144 RVA: 0x0000DC0A File Offset: 0x0000BE0A
		public int? BluePoint { get; private set; }

		// Token: 0x06000479 RID: 1145 RVA: 0x0000DC13 File Offset: 0x0000BE13
		public static IReadOnlyList<BluePointConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<BluePointConfig>(BluePointConfig._data);
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x0000DC24 File Offset: 0x0000BE24
		public static BluePointConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			BluePointConfig bluePointConfig;
			return (!BluePointConfig._IdTable.TryGetValue(Id, out bluePointConfig)) ? null : bluePointConfig;
		}

		// Token: 0x0600047B RID: 1147 RVA: 0x0000DC50 File Offset: 0x0000BE50
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{BluePointConfig Id=",
				ConfigDataManager.System_String.ToString(this.Id),
				", BluePoint=",
				(this.BluePoint == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.BluePoint.Value),
				"}"
			});
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x0000DCCC File Offset: 0x0000BECC
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				BluePointConfig[] array = new BluePointConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new BluePointConfig(ConfigDataManager.System_String.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)));
				}
				BluePointConfig._data = array;
				BluePointConfig._IdTable = Enumerable.ToDictionary<BluePointConfig, string>(BluePointConfig._data, (BluePointConfig elem) => elem.Id);
			}
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x0000DD98 File Offset: 0x0000BF98
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/BluePointConfig");
			if (textAsset != null)
			{
				try
				{
					BluePointConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load BluePointConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'BluePointConfig', please reimport config data");
			}
		}

		// Token: 0x0400025E RID: 606
		private static BluePointConfig[] _data = Array.Empty<BluePointConfig>();

		// Token: 0x0400025F RID: 607
		private static Dictionary<string, BluePointConfig> _IdTable = Enumerable.ToDictionary<BluePointConfig, string>(BluePointConfig._data, (BluePointConfig elem) => elem.Id);
	}
}
