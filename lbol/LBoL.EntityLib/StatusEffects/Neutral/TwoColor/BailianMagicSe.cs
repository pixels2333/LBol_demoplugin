using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x0200003F RID: 63
	[UsedImplicitly]
	public sealed class BailianMagicSe : StatusEffect
	{
		// Token: 0x060000C0 RID: 192 RVA: 0x000035CC File Offset: 0x000017CC
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsing, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsing));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00003608 File Offset: 0x00001808
		private IEnumerable<BattleAction> OnCardUsing(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Ability)
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

		// Token: 0x060000C2 RID: 194 RVA: 0x0000361F File Offset: 0x0000181F
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
