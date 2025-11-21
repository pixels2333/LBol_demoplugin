using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000078 RID: 120
	[UsedImplicitly]
	public sealed class KoishiPhoneSe : StatusEffect
	{
		// Token: 0x060001A1 RID: 417 RVA: 0x000053C4 File Offset: 0x000035C4
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			BattleController battle = base.Battle;
			int num = battle.FollowAttackFillerLevel + 1;
			battle.FollowAttackFillerLevel = num;
		}

		// Token: 0x060001A2 RID: 418 RVA: 0x00005403 File Offset: 0x00003603
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Card.CardType == CardType.Attack)
			{
				yield return new FollowAttackAction(args.Selector, base.Level, false);
			}
			yield break;
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x0000541C File Offset: 0x0000361C
		protected override void OnRemoved(Unit unit)
		{
			BattleController battle = base.Battle;
			int num = battle.FollowAttackFillerLevel - 1;
			battle.FollowAttackFillerLevel = num;
		}
	}
}
