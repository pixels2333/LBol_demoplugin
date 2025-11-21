using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000463 RID: 1123
	[UsedImplicitly]
	public sealed class FlowerGone : Card
	{
		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000F29 RID: 3881 RVA: 0x0001B50E File Offset: 0x0001970E
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodPeace>();
			}
		}

		// Token: 0x06000F2A RID: 3882 RVA: 0x0001B52A File Offset: 0x0001972A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.TriggeredAnyhow)
			{
				yield return base.DefenseAction(0, base.Shield.Shield, BlockShieldType.Normal, true);
			}
			else
			{
				yield return base.DefenseAction(this.Block.Block, 0, BlockShieldType.Normal, true);
			}
			yield break;
		}
	}
}
