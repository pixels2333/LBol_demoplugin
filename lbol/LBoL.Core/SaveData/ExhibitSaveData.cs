using System;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class ExhibitSaveData
	{
		public string Name;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? Counter;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? CardInstanceId;
	}
}
