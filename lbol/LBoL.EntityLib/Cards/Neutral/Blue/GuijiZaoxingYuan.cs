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
	// Token: 0x02000317 RID: 791
	[UsedImplicitly]
	public sealed class GuijiZaoxingYuan : Card
	{
		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000BB1 RID: 2993 RVA: 0x00017543 File Offset: 0x00015743
		public override bool RemoveFromBattleAfterPlay
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000BB2 RID: 2994 RVA: 0x00017546 File Offset: 0x00015746
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.AttackAction(selector, null);
			if (base.GameRun.BattleCardRng.NextInt(0, 1) == 0)
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingFang>(this.IsUpgraded) });
			}
			else
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingHu>(this.IsUpgraded) });
			}
			yield break;
		}
	}
}
