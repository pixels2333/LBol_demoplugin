using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000473 RID: 1139
	[UsedImplicitly]
	public sealed class KoishiFlower : Card
	{
		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000F4D RID: 3917 RVA: 0x0001B78E File Offset: 0x0001998E
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodEpiphany>();
			}
		}

		// Token: 0x06000F4E RID: 3918 RVA: 0x0001B7AA File Offset: 0x000199AA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.TriggeredAnyhow)
			{
				yield return new AddCardsToHandAction(Library.CreateCards<SummerFlower>(base.Value2, false), AddCardsType.Normal);
			}
			else
			{
				yield return new AddCardsToHandAction(Library.CreateCards<SummerFlower>(base.Value1, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
