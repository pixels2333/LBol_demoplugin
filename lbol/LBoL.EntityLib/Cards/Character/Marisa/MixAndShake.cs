using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class MixAndShake : Card
	{
		protected override int AdditionalDamage
		{
			get
			{
				if (base.Battle != null)
				{
					return base.Value1 * (Enumerable.Count<Potion>(Enumerable.OfType<Potion>(base.Battle.DrawZone)) + Enumerable.Count<Potion>(Enumerable.OfType<Potion>(base.Battle.DiscardZone)));
				}
				return 0;
			}
		}
	}
}
