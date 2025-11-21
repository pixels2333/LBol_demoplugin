using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003A4 RID: 932
	[UsedImplicitly]
	public sealed class SakuyaClock : Card
	{
		// Token: 0x06000D44 RID: 3396 RVA: 0x00019264 File Offset: 0x00017464
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x00019283 File Offset: 0x00017483
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (args.DieSource == this)
			{
				yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
				yield return new ExileCardAction(this);
				yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
				yield return new RequestEndPlayerTurnAction();
			}
			yield break;
		}
	}
}
