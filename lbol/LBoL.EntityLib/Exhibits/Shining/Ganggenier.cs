using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class Ganggenier : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
		}
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			Card card = args.ActionSource as Card;
			if (card != null && card.Config.Type == CardType.Attack)
			{
				DamageInfo damageInfo = args.DamageInfo;
				damageInfo.IsAccuracy = true;
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}
	}
}
