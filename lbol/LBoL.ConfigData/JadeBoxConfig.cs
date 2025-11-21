using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000012 RID: 18
	public sealed class JadeBoxConfig
	{
		// Token: 0x060002F1 RID: 753 RVA: 0x0000A014 File Offset: 0x00008214
		private JadeBoxConfig(int Index, string Id, int Order, IReadOnlyList<string> Group, int? Value1, int? Value2, int? Value3, ManaGroup? Mana, Keyword Keywords, IReadOnlyList<string> RelativeEffects, IReadOnlyList<string> RelativeCards)
		{
			this.Index = Index;
			this.Id = Id;
			this.Order = Order;
			this.Group = Group;
			this.Value1 = Value1;
			this.Value2 = Value2;
			this.Value3 = Value3;
			this.Mana = Mana;
			this.Keywords = Keywords;
			this.RelativeEffects = RelativeEffects;
			this.RelativeCards = RelativeCards;
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x060002F2 RID: 754 RVA: 0x0000A07C File Offset: 0x0000827C
		// (set) Token: 0x060002F3 RID: 755 RVA: 0x0000A084 File Offset: 0x00008284
		public int Index { get; private set; }

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x060002F4 RID: 756 RVA: 0x0000A08D File Offset: 0x0000828D
		// (set) Token: 0x060002F5 RID: 757 RVA: 0x0000A095 File Offset: 0x00008295
		public string Id { get; private set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x0000A09E File Offset: 0x0000829E
		// (set) Token: 0x060002F7 RID: 759 RVA: 0x0000A0A6 File Offset: 0x000082A6
		public int Order { get; private set; }

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060002F8 RID: 760 RVA: 0x0000A0AF File Offset: 0x000082AF
		// (set) Token: 0x060002F9 RID: 761 RVA: 0x0000A0B7 File Offset: 0x000082B7
		public IReadOnlyList<string> Group { get; private set; }

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060002FA RID: 762 RVA: 0x0000A0C0 File Offset: 0x000082C0
		// (set) Token: 0x060002FB RID: 763 RVA: 0x0000A0C8 File Offset: 0x000082C8
		public int? Value1 { get; private set; }

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x060002FC RID: 764 RVA: 0x0000A0D1 File Offset: 0x000082D1
		// (set) Token: 0x060002FD RID: 765 RVA: 0x0000A0D9 File Offset: 0x000082D9
		public int? Value2 { get; private set; }

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x060002FE RID: 766 RVA: 0x0000A0E2 File Offset: 0x000082E2
		// (set) Token: 0x060002FF RID: 767 RVA: 0x0000A0EA File Offset: 0x000082EA
		public int? Value3 { get; private set; }

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06000300 RID: 768 RVA: 0x0000A0F3 File Offset: 0x000082F3
		// (set) Token: 0x06000301 RID: 769 RVA: 0x0000A0FB File Offset: 0x000082FB
		public ManaGroup? Mana { get; private set; }

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x06000302 RID: 770 RVA: 0x0000A104 File Offset: 0x00008304
		// (set) Token: 0x06000303 RID: 771 RVA: 0x0000A10C File Offset: 0x0000830C
		public Keyword Keywords { get; private set; }

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x06000304 RID: 772 RVA: 0x0000A115 File Offset: 0x00008315
		// (set) Token: 0x06000305 RID: 773 RVA: 0x0000A11D File Offset: 0x0000831D
		public IReadOnlyList<string> RelativeEffects { get; private set; }

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000306 RID: 774 RVA: 0x0000A126 File Offset: 0x00008326
		// (set) Token: 0x06000307 RID: 775 RVA: 0x0000A12E File Offset: 0x0000832E
		public IReadOnlyList<string> RelativeCards { get; private set; }

		// Token: 0x06000308 RID: 776 RVA: 0x0000A137 File Offset: 0x00008337
		public static IReadOnlyList<JadeBoxConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<JadeBoxConfig>(JadeBoxConfig._data);
		}

		// Token: 0x06000309 RID: 777 RVA: 0x0000A148 File Offset: 0x00008348
		public static JadeBoxConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			JadeBoxConfig jadeBoxConfig;
			return (!JadeBoxConfig._IdTable.TryGetValue(Id, out jadeBoxConfig)) ? null : jadeBoxConfig;
		}

		// Token: 0x0600030A RID: 778 RVA: 0x0000A174 File Offset: 0x00008374
		public override string ToString()
		{
			string[] array = new string[23];
			array[0] = "{JadeBoxConfig Index=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Index);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", Order=";
			array[5] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[6] = ", Group=[";
			array[7] = string.Join(", ", Enumerable.Select<string, string>(this.Group, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[8] = "], Value1=";
			array[9] = ((this.Value1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value1.Value));
			array[10] = ", Value2=";
			array[11] = ((this.Value2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value2.Value));
			array[12] = ", Value3=";
			array[13] = ((this.Value3 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value3.Value));
			array[14] = ", Mana=";
			array[15] = ((this.Mana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Mana.Value));
			array[16] = ", Keywords=";
			array[17] = this.Keywords.ToString();
			array[18] = ", RelativeEffects=[";
			array[19] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[20] = "], RelativeCards=[";
			array[21] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[22] = "]}";
			return string.Concat(array);
		}

		// Token: 0x0600030B RID: 779 RVA: 0x0000A3D0 File Offset: 0x000085D0
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				JadeBoxConfig[] array = new JadeBoxConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new JadeBoxConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				JadeBoxConfig._data = array;
				JadeBoxConfig._IdTable = Enumerable.ToDictionary<JadeBoxConfig, string>(JadeBoxConfig._data, (JadeBoxConfig elem) => elem.Id);
			}
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0000A5A8 File Offset: 0x000087A8
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/JadeBoxConfig");
			if (textAsset != null)
			{
				try
				{
					JadeBoxConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load JadeBoxConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'JadeBoxConfig', please reimport config data");
			}
		}

		// Token: 0x04000189 RID: 393
		private static JadeBoxConfig[] _data = Array.Empty<JadeBoxConfig>();

		// Token: 0x0400018A RID: 394
		private static Dictionary<string, JadeBoxConfig> _IdTable = Enumerable.ToDictionary<JadeBoxConfig, string>(JadeBoxConfig._data, (JadeBoxConfig elem) => elem.Id);
	}
}
