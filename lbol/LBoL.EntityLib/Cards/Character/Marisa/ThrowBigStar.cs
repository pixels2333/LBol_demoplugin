using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class ThrowBigStar : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Overdrive(base.Value2))
			{
				yield return base.OverdriveAction(base.Value2);
				base.CardGuns = new Guns(new string[] { base.GunName, base.GunName });
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackAction(selector, gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			else
			{
				base.CardGuns = new Guns(base.GunName);
				yield return base.AttackAction(selector, null);
			}
			yield break;
			yield break;
		}
	}
}
