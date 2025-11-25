using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.White
{
	[UsedImplicitly]
	public sealed class NiuqiDefense : Card
	{
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return base.Battle.DiscardZone.Count;
			}
		}
	}
}
