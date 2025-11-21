using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000279 RID: 633
	[UsedImplicitly]
	public sealed class PrismriverWhite : Card
	{
		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000A07 RID: 2567 RVA: 0x000152B9 File Offset: 0x000134B9
		[UsedImplicitly]
		public int Light
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x06000A08 RID: 2568 RVA: 0x000152BC File Offset: 0x000134BC
		protected override string GetBaseDescription()
		{
			if (base.DebutActive || !this.IsUpgraded)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000A09 RID: 2569 RVA: 0x000152DB File Offset: 0x000134DB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield return new DrawManyCardAction(base.Value1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Riguang>(this.Light, false), AddCardsType.Normal);
			if (base.TriggeredAnyhow)
			{
				yield return new GainPowerAction(base.Value2);
			}
			yield break;
		}
	}
}
