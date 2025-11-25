using System;
using JetBrains.Annotations;
using LBoL.Base;
namespace LBoL.Core.GapOptions
{
	[UsedImplicitly]
	public sealed class RemoveCard : GapOption
	{
		public override GapOptionType Type
		{
			get
			{
				return GapOptionType.RemoveCard;
			}
		}
	}
}
