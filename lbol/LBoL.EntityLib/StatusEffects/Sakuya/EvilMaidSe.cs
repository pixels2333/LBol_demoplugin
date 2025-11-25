using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	[UsedImplicitly]
	public sealed class EvilMaidSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarting));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			Unit player = base.Battle.Player;
			int? num = new int?(base.Level);
			yield return new ApplyStatusEffectAction<EvilMaidDoubleAttack>(player, default(int?), num, default(int?), default(int?), 0f, true);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
