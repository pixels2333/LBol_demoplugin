using System;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x02000069 RID: 105
	public sealed class MasterOfCollectionSe : StatusEffect
	{
		// Token: 0x06000173 RID: 371 RVA: 0x00004E9C File Offset: 0x0000309C
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<CardEventArgs>(base.Battle.CardRetaining, new GameEventHandler<CardEventArgs>(this.OnCardRetaining));
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00004EBC File Offset: 0x000030BC
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
