using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000417 RID: 1047
	[UsedImplicitly]
	public sealed class CraftPotion : Card
	{
		// Token: 0x06000E67 RID: 3687 RVA: 0x0001A734 File Offset: 0x00018934
		protected override string GetBaseDescription()
		{
			if (!base.DebutActive)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x0001A74B File Offset: 0x0001894B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			if (this.IsUpgraded && base.TriggeredAnyhow)
			{
				yield return new DrawManyCardAction(base.Value2 + 1);
			}
			else
			{
				yield return new DrawManyCardAction(base.Value2);
			}
			yield break;
		}
	}
}
