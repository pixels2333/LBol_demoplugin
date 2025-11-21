using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x0200001C RID: 28
	public sealed class RuleConfig
	{
		// Token: 0x06000481 RID: 1153 RVA: 0x0000DE43 File Offset: 0x0000C043
		private RuleConfig(int Order, string Id, IReadOnlyList<string> CardIds, IReadOnlyList<string> ExhibitIds)
		{
			this.Order = Order;
			this.Id = Id;
			this.CardIds = CardIds;
			this.ExhibitIds = ExhibitIds;
		}

		// Token: 0x17000185 RID: 389
		// (get) Token: 0x06000482 RID: 1154 RVA: 0x0000DE68 File Offset: 0x0000C068
		// (set) Token: 0x06000483 RID: 1155 RVA: 0x0000DE70 File Offset: 0x0000C070
		public int Order { get; private set; }

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x06000484 RID: 1156 RVA: 0x0000DE79 File Offset: 0x0000C079
		// (set) Token: 0x06000485 RID: 1157 RVA: 0x0000DE81 File Offset: 0x0000C081
		public string Id { get; private set; }

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06000486 RID: 1158 RVA: 0x0000DE8A File Offset: 0x0000C08A
		// (set) Token: 0x06000487 RID: 1159 RVA: 0x0000DE92 File Offset: 0x0000C092
		public IReadOnlyList<string> CardIds { get; private set; }

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06000488 RID: 1160 RVA: 0x0000DE9B File Offset: 0x0000C09B
		// (set) Token: 0x06000489 RID: 1161 RVA: 0x0000DEA3 File Offset: 0x0000C0A3
		public IReadOnlyList<string> ExhibitIds { get; private set; }

		// Token: 0x0600048A RID: 1162 RVA: 0x0000DEAC File Offset: 0x0000C0AC
		public static IReadOnlyList<RuleConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<RuleConfig>(RuleConfig._data);
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x0000DEC0 File Offset: 0x0000C0C0
		public static RuleConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			RuleConfig ruleConfig;
			return (!RuleConfig._IdTable.TryGetValue(Id, out ruleConfig)) ? null : ruleConfig;
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x0000DEEC File Offset: 0x0000C0EC
		public override string ToString()
		{
			string[] array = new string[9];
			array[0] = "{RuleConfig Order=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", CardIds=[";
			array[5] = string.Join(", ", Enumerable.Select<string, string>(this.CardIds, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[6] = "], ExhibitIds=[";
			array[7] = string.Join(", ", Enumerable.Select<string, string>(this.ExhibitIds, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[8] = "]}";
			return string.Concat(array);
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x0000DFC0 File Offset: 0x0000C1C0
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				RuleConfig[] array = new RuleConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new RuleConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				RuleConfig._data = array;
				RuleConfig._IdTable = Enumerable.ToDictionary<RuleConfig, string>(RuleConfig._data, (RuleConfig elem) => elem.Id);
			}
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x0000E0B4 File Offset: 0x0000C2B4
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/RuleConfig");
			if (textAsset != null)
			{
				try
				{
					RuleConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load RuleConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'RuleConfig', please reimport config data");
			}
		}

		// Token: 0x04000265 RID: 613
		private static RuleConfig[] _data = Array.Empty<RuleConfig>();

		// Token: 0x04000266 RID: 614
		private static Dictionary<string, RuleConfig> _IdTable = Enumerable.ToDictionary<RuleConfig, string>(RuleConfig._data, (RuleConfig elem) => elem.Id);
	}
}
