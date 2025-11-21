using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x0200027F RID: 639
	[UsedImplicitly]
	public sealed class YuetuInvincible : Card
	{
		// Token: 0x06000A1B RID: 2587 RVA: 0x000154B3 File Offset: 0x000136B3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Invincible>(0, base.Value1, 0, 0, 0.2f);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<YuetuYuyi>(base.Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
