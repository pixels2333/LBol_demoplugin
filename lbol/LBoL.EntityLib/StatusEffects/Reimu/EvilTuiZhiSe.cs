using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x0200002A RID: 42
	[UsedImplicitly]
	public sealed class EvilTuiZhiSe : StatusEffect
	{
		// Token: 0x06000070 RID: 112 RVA: 0x00002BF5 File Offset: 0x00000DF5
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsing, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsing));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00002C31 File Offset: 0x00000E31
		private IEnumerable<BattleAction> OnCardUsing(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Attack)
			{
				base.NotifyActivating();
				args.PlayTwice = true;
				args.AddModifier(this);
				int num = base.Level - 1;
				base.Level = num;
				if (base.Level <= 0)
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00002C48 File Offset: 0x00000E48
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
