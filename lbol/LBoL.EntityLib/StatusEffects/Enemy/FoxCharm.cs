using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000A5 RID: 165
	[UsedImplicitly]
	public sealed class FoxCharm : StatusEffect
	{
		// Token: 0x0600024F RID: 591 RVA: 0x00006C14 File Offset: 0x00004E14
		protected override void OnAdded(Unit unit)
		{
			base.Count = base.Limit;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				base.Count = base.Limit;
				base.Highlight = false;
			});
			base.ReactOwnerEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
			base.HandleOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000250 RID: 592 RVA: 0x00006C89 File Offset: 0x00004E89
		private void OnCardUsed(CardUsingEventArgs args)
		{
			base.Count = base.Limit - base.Battle.TurnCardUsageHistory.Count;
			if (base.Count <= 1)
			{
				base.Highlight = true;
			}
		}

		// Token: 0x06000251 RID: 593 RVA: 0x00006CB8 File Offset: 0x00004EB8
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Unit is Fox)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x06000252 RID: 594 RVA: 0x00006CCF File Offset: 0x00004ECF
		public override bool ShouldPreventCardUsage(Card card)
		{
			return base.Count <= 0;
		}

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x06000253 RID: 595 RVA: 0x00006CDD File Offset: 0x00004EDD
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardCharm".Localize(true);
			}
		}
	}
}
