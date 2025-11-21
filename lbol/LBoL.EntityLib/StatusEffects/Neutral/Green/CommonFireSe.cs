using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Green
{
	// Token: 0x0200005A RID: 90
	[UsedImplicitly]
	public sealed class CommonFireSe : StatusEffect
	{
		// Token: 0x0600013A RID: 314 RVA: 0x000046D0 File Offset: 0x000028D0
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x0600013B RID: 315 RVA: 0x000046EF File Offset: 0x000028EF
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Card.Config.Rarity == Rarity.Common)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
