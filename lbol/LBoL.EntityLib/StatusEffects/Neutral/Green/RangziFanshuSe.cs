using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.Green
{
	public sealed class RangziFanshuSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(this.OnBattleEnding));
		}
		private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
		{
			if (base.Battle.Player.IsAlive)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Battle.Player, base.Battle.Player, base.Level, HealType.Normal, 0.2f);
			}
			yield break;
		}
	}
}
