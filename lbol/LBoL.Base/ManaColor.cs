using System;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.Base
{
	[ConfigValueConverter(typeof(ManaColorConverter), new string[] { })]
	public enum ManaColor
	{
		Any,
		White,
		Blue,
		Black,
		Red,
		Green,
		Colorless,
		Philosophy,
		Hybrid
	}
}
