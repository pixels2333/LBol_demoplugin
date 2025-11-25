using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LBoL.Base;
using UnityEngine;
namespace LBoL.ConfigData
{
	public sealed class ExhibitConfig
	{
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
		public int Index { get; private set; }
		public string Id { get; private set; }
		public int Order { get; private set; }
		public bool IsDebug { get; private set; }
		public bool IsPooled { get; private set; }
		public bool IsSentinel { get; private set; }
		public bool Revealable { get; private set; }
		public AppearanceType Appearance { get; private set; }
		public string Owner { get; private set; }
		public ExhibitLosableType LosableType { get; private set; }
		public Rarity Rarity { get; private set; }
		public int? Value1 { get; private set; }
		public int? Value2 { get; private set; }
		public int? Value3 { get; private set; }
		public ManaGroup? Mana { get; private set; }
		public ManaColor? BaseManaRequirement { get; private set; }
		public ManaColor? BaseManaColor { get; private set; }
		public int BaseManaAmount { get; private set; }
		public bool HasCounter { get; private set; }
		public int? InitialCounter { get; private set; }
		public Keyword Keywords { get; private set; }
		public IReadOnlyList<string> RelativeEffects { get; private set; }
		public IReadOnlyList<string> RelativeCards { get; private set; }
		public static IReadOnlyList<ExhibitConfig> AllConfig()
		{
			ConfigDataManager.Initialize();
			return Array.AsReadOnly<ExhibitConfig>(ExhibitConfig._data);
		}
		public static ExhibitConfig FromId(string Id)
		{
			ConfigDataManager.Initialize();
			ExhibitConfig exhibitConfig;
			return (!ExhibitConfig._IdTable.TryGetValue(Id, out exhibitConfig)) ? null : exhibitConfig;
		}
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
		private static ExhibitConfig[] _data = Array.Empty<ExhibitConfig>();
		private static Dictionary<string, ExhibitConfig> _IdTable = Enumerable.ToDictionary<ExhibitConfig, string>(ExhibitConfig._data, (ExhibitConfig elem) => elem.Id);
	}
}
