using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003B4 RID: 948
	[UsedImplicitly]
	public sealed class SakuyaUsage : Card
	{
		// Token: 0x06000D6E RID: 3438 RVA: 0x000194F4 File Offset: 0x000176F4
		protected override string GetBaseDescription()
		{
			if (base.PlayCount <= 0)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x1700017F RID: 383
		// (get) Token: 0x06000D6F RID: 3439 RVA: 0x0001950C File Offset: 0x0001770C
		public override bool Triggered
		{
			get
			{
				return base.PlayCount + 1 >= base.Value2;
			}
		}

		// Token: 0x06000D70 RID: 3440 RVA: 0x00019521 File Offset: 0x00017721
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.TriggeredAnyhow)
			{
				this.RemoveFromBattleAfterPlay = true;
				yield return new AddCardsToDrawZoneAction(Library.CreateCards<SakuyaUsage2>(1, false), DrawZoneTarget.Top, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
