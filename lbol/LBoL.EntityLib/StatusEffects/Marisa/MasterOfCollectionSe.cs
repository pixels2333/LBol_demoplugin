using System;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Marisa
{
	public sealed class MasterOfCollectionSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<CardEventArgs>(base.Battle.CardRetaining, new GameEventHandler<CardEventArgs>(this.OnCardRetaining));
		}
		private void OnCardRetaining(CardEventArgs args)
		{
			Card card = args.Card;
			if (card.Cost.Amount > 0)
			{
				ManaColor[] array = card.Cost.EnumerateComponents().SampleManyOrAll(base.Level, base.GameRun.BattleRng);
				card.DecreaseBaseCost(ManaGroup.FromComponents(array));
			}
		}
	}
}
