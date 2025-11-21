using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000316 RID: 790
	[UsedImplicitly]
	public sealed class GuijiZaoxingHu : Card
	{
		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000BAE RID: 2990 RVA: 0x00017521 File Offset: 0x00015721
		public override bool RemoveFromBattleAfterPlay
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000BAF RID: 2991 RVA: 0x00017524 File Offset: 0x00015724
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new DrawManyCardAction(base.Value1);
			if (base.GameRun.BattleCardRng.NextInt(0, 1) == 0)
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingYuan>(this.IsUpgraded) });
			}
			else
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingFang>(this.IsUpgraded) });
			}
			yield break;
		}
	}
}
