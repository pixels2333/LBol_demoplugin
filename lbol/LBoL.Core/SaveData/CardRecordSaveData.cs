using System;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class CardRecordSaveData
	{
		public string Id;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool Upgraded;
		public int? UpgradeCounter;
	}
}
