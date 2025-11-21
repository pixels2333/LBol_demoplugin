using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000BD RID: 189
	[UsedImplicitly]
	public sealed class PowerByDefense : StatusEffect
	{
		// Token: 0x06000297 RID: 663 RVA: 0x0000732C File Offset: 0x0000552C
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000734B File Offset: 0x0000554B
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || !base.Battle.Player.IsInTurn)
			{
				yield break;
			}
			if (args.Card.CardType == CardType.Defense)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
