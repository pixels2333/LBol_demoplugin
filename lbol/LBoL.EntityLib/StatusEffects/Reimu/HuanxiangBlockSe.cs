using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class HuanxiangBlockSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardExiled, new EventSequencedReactor<CardEventArgs>(this.OnCardExiled));
		}
		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Battle.Player, base.Level, 0, BlockShieldType.Direct, false);
			yield break;
		}
	}
}
