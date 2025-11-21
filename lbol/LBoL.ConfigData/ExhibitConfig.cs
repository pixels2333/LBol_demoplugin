using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;

namespace LBoL.ConfigData
{
	// Token: 0x02000011 RID: 17
	public sealed class ExhibitConfig
	{
		// Token: 0x060002B6 RID: 694 RVA: 0x000095FC File Offset: 0x000077FC
		private ExhibitConfig(int Index, string Id, int Order, bool IsDebug, bool IsPooled, bool IsSentinel, bool Revealable, AppearanceType Appearance, string Owner, ExhibitLosableType LosableType, Rarity Rarity, int? Value1, int? Value2, int? Value3, ManaGroup? Mana, ManaColor? BaseManaRequirement, ManaColor? BaseManaColor, int BaseManaAmount, bool HasCounter, int? InitialCounter, Keyword Keywords, IReadOnlyList<string> RelativeEffects, IReadOnlyList<string> RelativeCards)
		{
			this.Index = Index;
			this.Id = Id;
			this.Order = Order;
			this.IsDebug = IsDebug;
			this.IsPooled = IsPooled;
			this.IsSentinel = IsSentinel;
			this.Revealable = Revealable;
			this.Appearance = Appearance;
			this.Owner = Owner;
			this.LosableType = LosableType;
			this.Rarity = Rarity;
			this.Value1 = Value1;
			this.Value2 = Value2;
			this.Value3 = Value3;
			this.Mana = Mana;
			this.BaseManaRequirement = BaseManaRequirement;
			this.BaseManaColor = BaseManaColor;
			this.BaseManaAmount = BaseManaAmount;
			this.HasCounter = HasCounter;
			this.InitialCounter = InitialCounter;
			this.Keywords = Keywords;
			this.RelativeEffects = RelativeEffects;
			this.RelativeCards = RelativeCards;
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060002B7 RID: 695 RVA: 0x000096C4 File Offset: 0x000078C4
		// (set) Token: 0x060002B8 RID: 696 RVA: 0x000096CC File Offset: 0x000078CC
		public int Index { get; private set; }

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060002B9 RID: 697 RVA: 0x000096D5 File Offset: 0x000078D5
		// (set) Token: 0x060002BA RID: 698 RVA: 0x000096DD File Offset: 0x000078DD
		public string Id { get; private set; }

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060002BB RID: 699 RVA: 0x000096E6 File Offset: 0x000078E6
		// (set) Token: 0x060002BC RID: 700 RVA: 0x000096EE File Offset: 0x000078EE
		public int Order { get; private set; }

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060002BD RID: 701 RVA: 0x000096F7 File Offset: 0x000078F7
		// (set) Token: 0x060002BE RID: 702 RVA: 0x000096FF File Offset: 0x000078FF
		public bool IsDebug { get; private set; }

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060002BF RID: 703 RVA: 0x00009708 File Offset: 0x00007908
		// (set) Token: 0x060002C0 RID: 704 RVA: 0x00009710 File Offset: 0x00007910
		public bool IsPooled { get; private set; }

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060002C1 RID: 705 RVA: 0x00009719 File Offset: 0x00007919
		// (set) Token: 0x060002C2 RID: 706 RVA: 0x00009721 File Offset: 0x00007921
		public bool IsSentinel { get; private set; }

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060002C3 RID: 707 RVA: 0x0000972A File Offset: 0x0000792A
		// (set) Token: 0x060002C4 RID: 708 RVA: 0x00009732 File Offset: 0x00007932
		public bool Revealable { get; private set; }

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060002C5 RID: 709 RVA: 0x0000973B File Offset: 0x0000793B
		// (set) Token: 0x060002C6 RID: 710 RVA: 0x00009743 File Offset: 0x00007943
		public AppearanceType Appearance { get; private set; }

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060002C7 RID: 711 RVA: 0x0000974C File Offset: 0x0000794C
		// (set) Token: 0x060002C8 RID: 712 RVA: 0x00009754 File Offset: 0x00007954
		public string Owner { get; private set; }

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060002C9 RID: 713 RVA: 0x0000975D File Offset: 0x0000795D
		// (set) Token: 0x060002CA RID: 714 RVA: 0x00009765 File Offset: 0x00007965
		public ExhibitLosableType LosableType { get; private set; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060002CB RID: 715 RVA: 0x0000976E File Offset: 0x0000796E
		// (set) Token: 0x060002CC RID: 716 RVA: 0x00009776 File Offset: 0x00007976
		public Rarity Rarity { get; private set; }

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060002CD RID: 717 RVA: 0x0000977F File Offset: 0x0000797F
		// (set) Token: 0x060002CE RID: 718 RVA: 0x00009787 File Offset: 0x00007987
		public int? Value1 { get; private set; }

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060002CF RID: 719 RVA: 0x00009790 File Offset: 0x00007990
		// (set) Token: 0x060002D0 RID: 720 RVA: 0x00009798 File Offset: 0x00007998
		public int? Value2 { get; private set; }

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060002D1 RID: 721 RVA: 0x000097A1 File Offset: 0x000079A1
		// (set) Token: 0x060002D2 RID: 722 RVA: 0x000097A9 File Offset: 0x000079A9
		public int? Value3 { get; private set; }

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060002D3 RID: 723 RVA: 0x000097B2 File Offset: 0x000079B2
		// (set) Token: 0x060002D4 RID: 724 RVA: 0x000097BA File Offset: 0x000079BA
		public ManaGroup? Mana { get; private set; }

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x060002D5 RID: 725 RVA: 0x000097C3 File Offset: 0x000079C3
		// (set) Token: 0x060002D6 RID: 726 RVA: 0x000097CB File Offset: 0x000079CB
		public ManaColor? BaseManaRequirement { get; private set; }

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x060002D7 RID: 727 RVA: 0x000097D4 File Offset: 0x000079D4
		// (set) Token: 0x060002D8 RID: 728 RVA: 0x000097DC File Offset: 0x000079DC
		public ManaColor? BaseManaColor { get; private set; }

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x060002D9 RID: 729 RVA: 0x000097E5 File Offset: 0x000079E5
		// (set) Token: 0x060002DA RID: 730 RVA: 0x000097ED File Offset: 0x000079ED
		public int BaseManaAmount { get; private set; }

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060002DB RID: 731 RVA: 0x000097F6 File Offset: 0x000079F6
		// (set) Token: 0x060002DC RID: 732 RVA: 0x000097FE File Offset: 0x000079FE
		public bool HasCounter { get; private set; }

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x060002DD RID: 733 RVA: 0x00009807 File Offset: 0x00007A07
		// (set) Token: 0x060002DE RID: 734 RVA: 0x0000980F File Offset: 0x00007A0F
		public int? InitialCounter { get; private set; }

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x060002DF RID: 735 RVA: 0x00009818 File Offset: 0x00007A18
		// (set) Token: 0x060002E0 RID: 736 RVA: 0x00009820 File Offset: 0x00007A20
		public Keyword Keywords { get; private set; }

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x060002E1 RID: 737 RVA: 0x00009829 File Offset: 0x00007A29
		// (set) Token: 0x060002E2 RID: 738 RVA: 0x00009831 File Offset: 0x00007A31
		public IReadOnlyList<string> RelativeEffects { get; private set; }

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x060002E3 RID: 739 RVA: 0x0000983A File Offset: 0x00007A3A
		// (set) Token: 0x060002E4 RID: 740 RVA: 0x00009842 File Offset: 0x00007A42
		public IReadOnlyList<string> RelativeCards { get; private set; }

		// Token: 0x060002E5 RID: 741 RVA: 0x0000984B File Offset: 0x00007A4B
		public static IReadOnlyList<ExhibitConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<ExhibitConfig>(ExhibitConfig._data);
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x0000985C File Offset: 0x00007A5C
		public static ExhibitConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			ExhibitConfig exhibitConfig;
			return (!ExhibitConfig._IdTable.TryGetValue(Id, out exhibitConfig)) ? null : exhibitConfig;
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x00009888 File Offset: 0x00007A88
		public override string ToString()
		{
			string[] array = new string[47];
			array[0] = "{ExhibitConfig Index=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Index);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", Order=";
			array[5] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[6] = ", IsDebug=";
			array[7] = ConfigDataManager.System_Boolean.ToString(this.IsDebug);
			array[8] = ", IsPooled=";
			array[9] = ConfigDataManager.System_Boolean.ToString(this.IsPooled);
			array[10] = ", IsSentinel=";
			array[11] = ConfigDataManager.System_Boolean.ToString(this.IsSentinel);
			array[12] = ", Revealable=";
			array[13] = ConfigDataManager.System_Boolean.ToString(this.Revealable);
			array[14] = ", Appearance=";
			array[15] = this.Appearance.ToString();
			array[16] = ", Owner=";
			array[17] = ConfigDataManager.System_String.ToString(this.Owner);
			array[18] = ", LosableType=";
			array[19] = this.LosableType.ToString();
			array[20] = ", Rarity=";
			array[21] = this.Rarity.ToString();
			array[22] = ", Value1=";
			array[23] = ((this.Value1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value1.Value));
			array[24] = ", Value2=";
			array[25] = ((this.Value2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value2.Value));
			array[26] = ", Value3=";
			array[27] = ((this.Value3 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value3.Value));
			array[28] = ", Mana=";
			array[29] = ((this.Mana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Mana.Value));
			array[30] = ", BaseManaRequirement=";
			array[31] = ((this.BaseManaRequirement == null) ? "null" : ConfigDataManager.LBoL_Base_ManaColor.ToString(this.BaseManaRequirement.Value));
			array[32] = ", BaseManaColor=";
			array[33] = ((this.BaseManaColor == null) ? "null" : ConfigDataManager.LBoL_Base_ManaColor.ToString(this.BaseManaColor.Value));
			array[34] = ", BaseManaAmount=";
			array[35] = ConfigDataManager.System_Int32.ToString(this.BaseManaAmount);
			array[36] = ", HasCounter=";
			array[37] = ConfigDataManager.System_Boolean.ToString(this.HasCounter);
			array[38] = ", InitialCounter=";
			array[39] = ((this.InitialCounter == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.InitialCounter.Value));
			array[40] = ", Keywords=";
			array[41] = this.Keywords.ToString();
			array[42] = ", RelativeEffects=[";
			array[43] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[44] = "], RelativeCards=[";
			array[45] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[46] = "]}";
			return string.Concat(array);
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x00009CA4 File Offset: 0x00007EA4
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				ExhibitConfig[] array = new ExhibitConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new ExhibitConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (AppearanceType)binaryReader.ReadInt32(), ConfigDataManager.System_String.ReadFrom(binaryReader), (ExhibitLosableType)binaryReader.ReadInt32(), (Rarity)binaryReader.ReadInt32(), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaColor?(ConfigDataManager.LBoL_Base_ManaColor.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaColor?(ConfigDataManager.LBoL_Base_ManaColor.ReadFrom(binaryReader)), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				ExhibitConfig._data = array;
				ExhibitConfig._IdTable = Enumerable.ToDictionary<ExhibitConfig, string>(ExhibitConfig._data, (ExhibitConfig elem) => elem.Id);
			}
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x00009F34 File Offset: 0x00008134
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/ExhibitConfig");
			if (textAsset != null)
			{
				try
				{
					ExhibitConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load ExhibitConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'ExhibitConfig', please reimport config data");
			}
		}

		// Token: 0x04000177 RID: 375
		private static ExhibitConfig[] _data = Array.Empty<ExhibitConfig>();

		// Token: 0x04000178 RID: 376
		private static Dictionary<string, ExhibitConfig> _IdTable = Enumerable.ToDictionary<ExhibitConfig, string>(ExhibitConfig._data, (ExhibitConfig elem) => elem.Id);
	}
}
