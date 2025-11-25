using System;
using System.Collections.Generic;
using System.ComponentModel;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	[Serializable]
	public sealed class HintStatusSaveData
	{
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[DefaultValue(false)]
		public bool BattleHintShown;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public HashSet<string> ShownHints = new HashSet<string>();
	}
}
