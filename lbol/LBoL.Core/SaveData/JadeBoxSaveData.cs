using System;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class JadeBoxSaveData
	{
		public string Name;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? Counter;
	}
}
