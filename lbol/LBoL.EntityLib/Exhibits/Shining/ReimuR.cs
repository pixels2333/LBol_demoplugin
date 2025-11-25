using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class ReimuR : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			base.NotifyActivating();
			if (!base.Battle.BattleShouldEnd)
			{
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield return new HealAction(base.Owner, base.Owner, base.Value2, HealType.Normal, 0.2f);
			yield break;
		}
	}
}
