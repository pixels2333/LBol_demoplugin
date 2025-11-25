using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Adventure
{
	[UsedImplicitly]
	public sealed class WolfFur : Card
	{
		public override bool Negative
		{
			get
			{
				return true;
			}
		}
	}
}
