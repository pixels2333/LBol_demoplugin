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
	// Token: 0x020002AD RID: 685
	[UsedImplicitly]
	public sealed class Shengyan : Card
	{
		// Token: 0x06000A95 RID: 2709 RVA: 0x00015DB9 File Offset: 0x00013FB9
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x06000A96 RID: 2710 RVA: 0x00015DD8 File Offset: 0x00013FD8
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
