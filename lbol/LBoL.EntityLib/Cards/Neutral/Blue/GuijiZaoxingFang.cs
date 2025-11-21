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
	// Token: 0x02000315 RID: 789
	[UsedImplicitly]
	public sealed class GuijiZaoxingFang : Card
	{
		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000BAB RID: 2987 RVA: 0x000174FF File Offset: 0x000156FF
		public override bool RemoveFromBattleAfterPlay
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x00017502 File Offset: 0x00015702
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			if (base.GameRun.BattleCardRng.NextInt(0, 1) == 0)
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingYuan>(this.IsUpgraded) });
			}
			else
			{
				yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<GuijiZaoxingHu>(this.IsUpgraded) });
			}
			yield break;
			yield break;
		}
	}
}
