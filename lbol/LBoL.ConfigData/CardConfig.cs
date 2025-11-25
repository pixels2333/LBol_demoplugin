using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class CardConfig
	{
		private CardConfig(int Index, string Id, int Order, bool AutoPerform, string[][] Perform, string GunName, string GunNameBurst, int DebugLevel, bool Revealable, bool IsPooled, bool HideMesuem, bool FindInBattle, bool IsUpgradable, Rarity Rarity, CardType Type, TargetType? TargetType, IReadOnlyList<ManaColor> Colors, bool IsXCost, ManaGroup Cost, ManaGroup? UpgradedCost, ManaGroup? Kicker, ManaGroup? UpgradedKicker, int? MoneyCost, int? Damage, int? UpgradedDamage, int? Block, int? UpgradedBlock, int? Shield, int? UpgradedShield, int? Value1, int? UpgradedValue1, int? Value2, int? UpgradedValue2, ManaGroup? Mana, ManaGroup? UpgradedMana, int? Scry, int? UpgradedScry, int? ToolPlayableTimes, int? Loyalty, int? UpgradedLoyalty, int? PassiveCost, int? UpgradedPassiveCost, int? ActiveCost, int? UpgradedActiveCost, int? ActiveCost2, int? UpgradedActiveCost2, int? UltimateCost, int? UpgradedUltimateCost, Keyword Keywords, Keyword UpgradedKeywords, bool EmptyDescription, Keyword RelativeKeyword, Keyword UpgradedRelativeKeyword, IReadOnlyList<string> RelativeEffects, IReadOnlyList<string> UpgradedRelativeEffects, IReadOnlyList<string> RelativeCards, IReadOnlyList<string> UpgradedRelativeCards, string Owner, string ImageId, string UpgradeImageId, bool Unfinished, string Illustrator, IReadOnlyList<string> SubIllustrator)
		{
			this.Index = Index;
			this.Id = Id;
			this.Order = Order;
			this.AutoPerform = AutoPerform;
			this.Perform = Perform;
			this.GunName = GunName;
			this.GunNameBurst = GunNameBurst;
			this.DebugLevel = DebugLevel;
			this.Revealable = Revealable;
			this.IsPooled = IsPooled;
			this.HideMesuem = HideMesuem;
			this.FindInBattle = FindInBattle;
			this.IsUpgradable = IsUpgradable;
			this.Rarity = Rarity;
			this.Type = Type;
			this.TargetType = TargetType;
			this.Colors = Colors;
			this.IsXCost = IsXCost;
			this.Cost = Cost;
			this.UpgradedCost = UpgradedCost;
			this.Kicker = Kicker;
			this.UpgradedKicker = UpgradedKicker;
			this.MoneyCost = MoneyCost;
			this.Damage = Damage;
			this.UpgradedDamage = UpgradedDamage;
			this.Block = Block;
			this.UpgradedBlock = UpgradedBlock;
			this.Shield = Shield;
			this.UpgradedShield = UpgradedShield;
			this.Value1 = Value1;
			this.UpgradedValue1 = UpgradedValue1;
			this.Value2 = Value2;
			this.UpgradedValue2 = UpgradedValue2;
			this.Mana = Mana;
			this.UpgradedMana = UpgradedMana;
			this.Scry = Scry;
			this.UpgradedScry = UpgradedScry;
			this.ToolPlayableTimes = ToolPlayableTimes;
			this.Loyalty = Loyalty;
			this.UpgradedLoyalty = UpgradedLoyalty;
			this.PassiveCost = PassiveCost;
			this.UpgradedPassiveCost = UpgradedPassiveCost;
			this.ActiveCost = ActiveCost;
			this.UpgradedActiveCost = UpgradedActiveCost;
			this.ActiveCost2 = ActiveCost2;
			this.UpgradedActiveCost2 = UpgradedActiveCost2;
			this.UltimateCost = UltimateCost;
			this.UpgradedUltimateCost = UpgradedUltimateCost;
			this.Keywords = Keywords;
			this.UpgradedKeywords = UpgradedKeywords;
			this.EmptyDescription = EmptyDescription;
			this.RelativeKeyword = RelativeKeyword;
			this.UpgradedRelativeKeyword = UpgradedRelativeKeyword;
			this.RelativeEffects = RelativeEffects;
			this.UpgradedRelativeEffects = UpgradedRelativeEffects;
			this.RelativeCards = RelativeCards;
			this.UpgradedRelativeCards = UpgradedRelativeCards;
			this.Owner = Owner;
			this.ImageId = ImageId;
			this.UpgradeImageId = UpgradeImageId;
			this.Unfinished = Unfinished;
			this.Illustrator = Illustrator;
			this.SubIllustrator = SubIllustrator;
		}
		public int Index { get; private set; }
		public string Id { get; private set; }
		public int Order { get; private set; }
		public bool AutoPerform { get; private set; }
		public string[][] Perform { get; private set; }
		public string GunName { get; private set; }
		public string GunNameBurst { get; private set; }
		public int DebugLevel { get; private set; }
		public bool Revealable { get; private set; }
		public bool IsPooled { get; private set; }
		public bool HideMesuem { get; private set; }
		public bool FindInBattle { get; private set; }
		public bool IsUpgradable { get; private set; }
		public Rarity Rarity { get; private set; }
		public CardType Type { get; private set; }
		public TargetType? TargetType { get; private set; }
		public IReadOnlyList<ManaColor> Colors { get; private set; }
		public bool IsXCost { get; private set; }
		public ManaGroup Cost { get; private set; }
		public ManaGroup? UpgradedCost { get; private set; }
		public ManaGroup? Kicker { get; private set; }
		public ManaGroup? UpgradedKicker { get; private set; }
		public int? MoneyCost { get; private set; }
		public int? Damage { get; private set; }
		public int? UpgradedDamage { get; private set; }
		public int? Block { get; private set; }
		public int? UpgradedBlock { get; private set; }
		public int? Shield { get; private set; }
		public int? UpgradedShield { get; private set; }
		public int? Value1 { get; private set; }
		public int? UpgradedValue1 { get; private set; }
		public int? Value2 { get; private set; }
		public int? UpgradedValue2 { get; private set; }
		public ManaGroup? Mana { get; private set; }
		public ManaGroup? UpgradedMana { get; private set; }
		public int? Scry { get; private set; }
		public int? UpgradedScry { get; private set; }
		public int? ToolPlayableTimes { get; private set; }
		public int? Loyalty { get; private set; }
		public int? UpgradedLoyalty { get; private set; }
		public int? PassiveCost { get; private set; }
		public int? UpgradedPassiveCost { get; private set; }
		public int? ActiveCost { get; private set; }
		public int? UpgradedActiveCost { get; private set; }
		public int? ActiveCost2 { get; private set; }
		public int? UpgradedActiveCost2 { get; private set; }
		public int? UltimateCost { get; private set; }
		public int? UpgradedUltimateCost { get; private set; }
		public Keyword Keywords { get; private set; }
		public Keyword UpgradedKeywords { get; private set; }
		public bool EmptyDescription { get; private set; }
		public Keyword RelativeKeyword { get; private set; }
		public Keyword UpgradedRelativeKeyword { get; private set; }
		public IReadOnlyList<string> RelativeEffects { get; private set; }
		public IReadOnlyList<string> UpgradedRelativeEffects { get; private set; }
		public IReadOnlyList<string> RelativeCards { get; private set; }
		public IReadOnlyList<string> UpgradedRelativeCards { get; private set; }
		public string Owner { get; private set; }
		public string ImageId { get; private set; }
		public string UpgradeImageId { get; private set; }
		public bool Unfinished { get; private set; }
		public string Illustrator { get; private set; }
		public IReadOnlyList<string> SubIllustrator { get; private set; }
		public static IReadOnlyList<CardConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<CardConfig>(CardConfig._data);
		}
		public static CardConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			CardConfig cardConfig;
			return (!CardConfig._IdTable.TryGetValue(Id, out cardConfig)) ? null : cardConfig;
		}
		public override string ToString()
		{
			string[] array = new string[127];
			array[0] = "{CardConfig Index=";
			array[1] = ConfigDataManager.System_Int32.ToString(this.Index);
			array[2] = ", Id=";
			array[3] = ConfigDataManager.System_String.ToString(this.Id);
			array[4] = ", Order=";
			array[5] = ConfigDataManager.System_Int32.ToString(this.Order);
			array[6] = ", AutoPerform=";
			array[7] = ConfigDataManager.System_Boolean.ToString(this.AutoPerform);
			array[8] = ", Perform=[";
			array[9] = string.Join(", ", Enumerable.Select<string[], string>(this.Perform, (string[] v2) => "[" + string.Join(", ", Enumerable.Select<string, string>(v2, (string v1) => ConfigDataManager.System_String.ToString(v1))) + "]"));
			array[10] = "], GunName=";
			array[11] = ConfigDataManager.System_String.ToString(this.GunName);
			array[12] = ", GunNameBurst=";
			array[13] = ConfigDataManager.System_String.ToString(this.GunNameBurst);
			array[14] = ", DebugLevel=";
			array[15] = ConfigDataManager.System_Int32.ToString(this.DebugLevel);
			array[16] = ", Revealable=";
			array[17] = ConfigDataManager.System_Boolean.ToString(this.Revealable);
			array[18] = ", IsPooled=";
			array[19] = ConfigDataManager.System_Boolean.ToString(this.IsPooled);
			array[20] = ", HideMesuem=";
			array[21] = ConfigDataManager.System_Boolean.ToString(this.HideMesuem);
			array[22] = ", FindInBattle=";
			array[23] = ConfigDataManager.System_Boolean.ToString(this.FindInBattle);
			array[24] = ", IsUpgradable=";
			array[25] = ConfigDataManager.System_Boolean.ToString(this.IsUpgradable);
			array[26] = ", Rarity=";
			array[27] = this.Rarity.ToString();
			array[28] = ", Type=";
			array[29] = this.Type.ToString();
			array[30] = ", TargetType=";
			array[31] = ((this.TargetType == null) ? "null" : this.TargetType.Value.ToString());
			array[32] = ", Colors=[";
			array[33] = string.Join(", ", Enumerable.Select<ManaColor, string>(this.Colors, (ManaColor v1) => ConfigDataManager.LBoL_Base_ManaColor.ToString(v1)));
			array[34] = "], IsXCost=";
			array[35] = ConfigDataManager.System_Boolean.ToString(this.IsXCost);
			array[36] = ", Cost=";
			array[37] = ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Cost);
			array[38] = ", UpgradedCost=";
			array[39] = ((this.UpgradedCost == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.UpgradedCost.Value));
			array[40] = ", Kicker=";
			array[41] = ((this.Kicker == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Kicker.Value));
			array[42] = ", UpgradedKicker=";
			array[43] = ((this.UpgradedKicker == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.UpgradedKicker.Value));
			array[44] = ", MoneyCost=";
			array[45] = ((this.MoneyCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.MoneyCost.Value));
			array[46] = ", Damage=";
			array[47] = ((this.Damage == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Damage.Value));
			array[48] = ", UpgradedDamage=";
			array[49] = ((this.UpgradedDamage == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedDamage.Value));
			array[50] = ", Block=";
			array[51] = ((this.Block == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Block.Value));
			array[52] = ", UpgradedBlock=";
			array[53] = ((this.UpgradedBlock == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedBlock.Value));
			array[54] = ", Shield=";
			array[55] = ((this.Shield == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Shield.Value));
			array[56] = ", UpgradedShield=";
			array[57] = ((this.UpgradedShield == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedShield.Value));
			array[58] = ", Value1=";
			array[59] = ((this.Value1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value1.Value));
			array[60] = ", UpgradedValue1=";
			array[61] = ((this.UpgradedValue1 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedValue1.Value));
			array[62] = ", Value2=";
			array[63] = ((this.Value2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Value2.Value));
			array[64] = ", UpgradedValue2=";
			array[65] = ((this.UpgradedValue2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedValue2.Value));
			array[66] = ", Mana=";
			array[67] = ((this.Mana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.Mana.Value));
			array[68] = ", UpgradedMana=";
			array[69] = ((this.UpgradedMana == null) ? "null" : ConfigDataManager.LBoL_Base_ManaGroup.ToString(this.UpgradedMana.Value));
			array[70] = ", Scry=";
			array[71] = ((this.Scry == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Scry.Value));
			array[72] = ", UpgradedScry=";
			array[73] = ((this.UpgradedScry == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedScry.Value));
			array[74] = ", ToolPlayableTimes=";
			array[75] = ((this.ToolPlayableTimes == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.ToolPlayableTimes.Value));
			array[76] = ", Loyalty=";
			array[77] = ((this.Loyalty == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.Loyalty.Value));
			array[78] = ", UpgradedLoyalty=";
			array[79] = ((this.UpgradedLoyalty == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedLoyalty.Value));
			array[80] = ", PassiveCost=";
			array[81] = ((this.PassiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.PassiveCost.Value));
			array[82] = ", UpgradedPassiveCost=";
			array[83] = ((this.UpgradedPassiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedPassiveCost.Value));
			array[84] = ", ActiveCost=";
			array[85] = ((this.ActiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.ActiveCost.Value));
			array[86] = ", UpgradedActiveCost=";
			array[87] = ((this.UpgradedActiveCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedActiveCost.Value));
			array[88] = ", ActiveCost2=";
			array[89] = ((this.ActiveCost2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.ActiveCost2.Value));
			array[90] = ", UpgradedActiveCost2=";
			array[91] = ((this.UpgradedActiveCost2 == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedActiveCost2.Value));
			array[92] = ", UltimateCost=";
			array[93] = ((this.UltimateCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UltimateCost.Value));
			array[94] = ", UpgradedUltimateCost=";
			array[95] = ((this.UpgradedUltimateCost == null) ? "null" : ConfigDataManager.System_Int32.ToString(this.UpgradedUltimateCost.Value));
			array[96] = ", Keywords=";
			array[97] = this.Keywords.ToString();
			array[98] = ", UpgradedKeywords=";
			array[99] = this.UpgradedKeywords.ToString();
			array[100] = ", EmptyDescription=";
			array[101] = ConfigDataManager.System_Boolean.ToString(this.EmptyDescription);
			array[102] = ", RelativeKeyword=";
			array[103] = this.RelativeKeyword.ToString();
			array[104] = ", UpgradedRelativeKeyword=";
			array[105] = this.UpgradedRelativeKeyword.ToString();
			array[106] = ", RelativeEffects=[";
			array[107] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[108] = "], UpgradedRelativeEffects=[";
			array[109] = string.Join(", ", Enumerable.Select<string, string>(this.UpgradedRelativeEffects, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[110] = "], RelativeCards=[";
			array[111] = string.Join(", ", Enumerable.Select<string, string>(this.RelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[112] = "], UpgradedRelativeCards=[";
			array[113] = string.Join(", ", Enumerable.Select<string, string>(this.UpgradedRelativeCards, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[114] = "], Owner=";
			array[115] = ConfigDataManager.System_String.ToString(this.Owner);
			array[116] = ", ImageId=";
			array[117] = ConfigDataManager.System_String.ToString(this.ImageId);
			array[118] = ", UpgradeImageId=";
			array[119] = ConfigDataManager.System_String.ToString(this.UpgradeImageId);
			array[120] = ", Unfinished=";
			array[121] = ConfigDataManager.System_Boolean.ToString(this.Unfinished);
			array[122] = ", Illustrator=";
			array[123] = ConfigDataManager.System_String.ToString(this.Illustrator);
			array[124] = ", SubIllustrator=[";
			array[125] = string.Join(", ", Enumerable.Select<string, string>(this.SubIllustrator, (string v1) => ConfigDataManager.System_String.ToString(v1)));
			array[126] = "]}";
			return string.Concat(array);
		}
		private static void Load(byte[] bytes)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes)))
			{
				CardConfig[] array = new CardConfig[binaryReader.ReadInt32()];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new CardConfig(ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.ReadArray<string[]>(binaryReader, (BinaryReader r2) => ConfigDataManager.ReadArray<string>(r2, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1))), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Int32.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (Rarity)binaryReader.ReadInt32(), (CardType)binaryReader.ReadInt32(), (!binaryReader.ReadBoolean()) ? null : new TargetType?((TargetType)binaryReader.ReadInt32()), ConfigDataManager.ReadList<ManaColor>(binaryReader, (BinaryReader r1) => ConfigDataManager.LBoL_Base_ManaColor.ReadFrom(r1)), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new ManaGroup?(ConfigDataManager.LBoL_Base_ManaGroup.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (!binaryReader.ReadBoolean()) ? null : new int?(ConfigDataManager.System_Int32.ReadFrom(binaryReader)), (Keyword)binaryReader.ReadInt64(), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), (Keyword)binaryReader.ReadInt64(), (Keyword)binaryReader.ReadInt64(), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.System_Boolean.ReadFrom(binaryReader), ConfigDataManager.System_String.ReadFrom(binaryReader), ConfigDataManager.ReadList<string>(binaryReader, (BinaryReader r1) => ConfigDataManager.System_String.ReadFrom(r1)));
				}
				CardConfig._data = array;
				CardConfig._IdTable = Enumerable.ToDictionary<CardConfig, string>(CardConfig._data, (CardConfig elem) => elem.Id);
			}
		}
		internal static void Reload()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ConfigData/CardConfig");
			if (textAsset != null)
			{
				try
				{
					CardConfig.Load(textAsset.bytes);
				}
				catch (Exception ex)
				{
					Debug.LogError(string.Format("Failed to load CardConfig data: {0}, try reimport config data", ex.Message));
				}
				Resources.UnloadAsset(textAsset);
			}
			else
			{
				Debug.LogError("Cannot load config data of 'CardConfig', please reimport config data");
			}
		}
		private static CardConfig[] _data = Array.Empty<CardConfig>();
		private static Dictionary<string, CardConfig> _IdTable = Enumerable.ToDictionary<CardConfig, string>(CardConfig._data, (CardConfig elem) => elem.Id);
	}
}
