using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200047B RID: 1147
	[UsedImplicitly]
	public sealed class KoishiSpringOut : Card
	{
		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000F5E RID: 3934 RVA: 0x0001B8A7 File Offset: 0x00019AA7
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodPeace>();
			}
		}

		// Token: 0x06000F5F RID: 3935 RVA: 0x0001B8C3 File Offset: 0x00019AC3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.TriggeredAnyhow)
			{
				yield return new DrawManyCardAction(base.Value1);
			}
			else
			{
				yield return base.BuffAction<MoodPeace>(0, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
