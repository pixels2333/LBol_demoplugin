using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x02000110 RID: 272
	[UsedImplicitly]
	public sealed class DefensePhilosophy : JadeBox
	{
		// Token: 0x060003B9 RID: 953 RVA: 0x0000A60E File Offset: 0x0000880E
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x060003BA RID: 954 RVA: 0x0000A62D File Offset: 0x0000882D
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Defense)
			{
				yield return new GainManaAction(base.Mana);
				if (base.Battle.HandZone.Count > 0)
				{
					yield return new DiscardAction(Enumerable.First<Card>(base.Battle.HandZone));
				}
			}
			yield break;
		}
	}
}
