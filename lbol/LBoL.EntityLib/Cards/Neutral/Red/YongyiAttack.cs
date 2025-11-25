using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Neutral.Red
{
	[UsedImplicitly]
	public sealed class YongyiAttack : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = base.Value1;
			if (base.Battle.Player.HasStatusEffect<TempFirepower>())
			{
				num++;
			}
			Guns guns = new Guns(base.GunName, num, true);
			foreach (GunPair gunPair in guns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield break;
			yield break;
		}
	}
}
