using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000094 RID: 148
	[UsedImplicitly]
	public sealed class DeepFreezeSe : StatusEffect
	{
		// Token: 0x0600074D RID: 1869 RVA: 0x00015A60 File Offset: 0x00013C60
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x00015A7F File Offset: 0x00013C7F
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (Enumerable.Contains<ManaColor>(args.Card.Config.Colors, ManaColor.Blue))
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
