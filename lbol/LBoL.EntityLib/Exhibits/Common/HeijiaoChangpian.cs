using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200016C RID: 364
	[UsedImplicitly]
	public sealed class HeijiaoChangpian : Exhibit
	{
		// Token: 0x06000509 RID: 1289 RVA: 0x0000CAE3 File Offset: 0x0000ACE3
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
		}

		// Token: 0x0600050A RID: 1290 RVA: 0x0000CB02 File Offset: 0x0000AD02
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (args.Card.CardType == CardType.Status)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
