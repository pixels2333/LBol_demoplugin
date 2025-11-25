using System;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class CardSaveData
	{
		public string Name;
		public int InstanceId;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool IsUpgraded;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? DeckCounter;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? UpgradeCounter;
	}
}
