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
	// Token: 0x02000445 RID: 1093
	[UsedImplicitly]
	public sealed class ShiningPotion : Card
	{
		// Token: 0x06000EE5 RID: 3813 RVA: 0x0001B0DD File Offset: 0x000192DD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.Overdrive(base.Value2))
			{
				yield return base.OverdriveAction(base.Value2);
				yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
			}
			else
			{
				yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
