using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
namespace LBoL.EntityLib.Exhibits.Mythic
{
	[UsedImplicitly]
	public sealed class FoyushiBo : MythicExhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsing, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
		}
		private void OnCardUsing(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Defense)
			{
				base.NotifyActivating();
				args.PlayTwice = true;
				args.AddModifier(this);
			}
		}
	}
}
