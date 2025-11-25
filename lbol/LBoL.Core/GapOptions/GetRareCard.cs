using System;
using JetBrains.Annotations;
using LBoL.Base;
namespace LBoL.Core.GapOptions
{
	[UsedImplicitly]
	public sealed class GetRareCard : GapOption
	{
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.GetRareCard;
			}
		}
		[UsedImplicitly]
		public int Value { get; set; }
	}
}
