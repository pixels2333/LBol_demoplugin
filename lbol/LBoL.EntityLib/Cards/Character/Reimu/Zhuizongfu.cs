using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class Zhuizongfu : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new GainTurnManaAction(base.Mana);
			yield break;
			yield break;
		}
	}
}
