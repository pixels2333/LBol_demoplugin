using System;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class SysSaveData
	{
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? SaveIndex;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string Locale;
	}
}
