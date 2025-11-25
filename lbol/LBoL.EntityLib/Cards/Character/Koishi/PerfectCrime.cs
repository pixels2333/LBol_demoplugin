using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class PerfectCrime : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			PerfectCrime.<>c__DisplayClass0_0 CS$<>8__locals1 = new PerfectCrime.<>c__DisplayClass0_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.enemy = selector.SelectedEnemy;
			if (CS$<>8__locals1.enemy.Shield > 0)
			{
				int shield = CS$<>8__locals1.enemy.Shield;
				yield return new LoseBlockShieldAction(CS$<>8__locals1.enemy, 0, shield, false);
				yield return base.DefenseAction(0, shield, BlockShieldType.Direct, false);
			}
			foreach (BattleAction battleAction in CS$<>8__locals1.<Actions>g__StealActions|0<Graze>(0))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			foreach (BattleAction battleAction2 in CS$<>8__locals1.<Actions>g__StealActions|0<GuangxueMicai>(0))
			{
				yield return battleAction2;
			}
			enumerator = null;
			foreach (BattleAction battleAction3 in CS$<>8__locals1.<Actions>g__StealActions|0<Invincible>(0))
			{
				yield return battleAction3;
			}
			enumerator = null;
			foreach (BattleAction battleAction4 in CS$<>8__locals1.<Actions>g__StealActions|0<InvincibleEternal>(0))
			{
				yield return battleAction4;
			}
			enumerator = null;
			foreach (BattleAction battleAction5 in CS$<>8__locals1.<Actions>g__StealActions|0<Firepower>(base.Value1))
			{
				yield return battleAction5;
			}
			enumerator = null;
			foreach (BattleAction battleAction6 in CS$<>8__locals1.<Actions>g__StealActions|0<TempFirepower>(base.Value1))
			{
				yield return battleAction6;
			}
			enumerator = null;
			foreach (BattleAction battleAction7 in CS$<>8__locals1.<Actions>g__StealActions|0<Spirit>(base.Value1))
			{
				yield return battleAction7;
			}
			enumerator = null;
			foreach (BattleAction battleAction8 in CS$<>8__locals1.<Actions>g__StealActions|0<TempSpirit>(base.Value1))
			{
				yield return battleAction8;
			}
			enumerator = null;
			yield break;
			yield break;
		}
	}
}
