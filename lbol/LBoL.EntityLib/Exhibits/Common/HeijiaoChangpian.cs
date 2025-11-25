using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class HeijiaoChangpian : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
		}
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
