using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class Shengyan : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (args.DieSource == this && !args.Unit.HasStatusEffect<Servant>())
			{
				base.GameRun.GainMaxHp(base.Value1, true, true);
				yield return PerformAction.Sfx("Shengyan", 0f);
			}
			yield break;
		}
	}
}
