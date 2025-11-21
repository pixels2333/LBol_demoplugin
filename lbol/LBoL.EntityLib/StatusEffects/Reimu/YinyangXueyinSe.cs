using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Reimu;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000038 RID: 56
	[UsedImplicitly]
	public sealed class YinyangXueyinSe : StatusEffect
	{
		// Token: 0x060000A5 RID: 165 RVA: 0x00003289 File Offset: 0x00001489
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x000032A8 File Offset: 0x000014A8
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Card is YinyangCardBase)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
				yield return new ApplyStatusEffectAction<TempSpirit>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
