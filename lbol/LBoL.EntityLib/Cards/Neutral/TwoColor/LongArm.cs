using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000299 RID: 665
	[UsedImplicitly]
	public sealed class LongArm : Card
	{
		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000A62 RID: 2658 RVA: 0x00015A4B File Offset: 0x00013C4B
		public override bool Triggered
		{
			get
			{
				return Enumerable.Count<Card>(base.Battle.HandZone) >= base.Value1;
			}
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x00015A68 File Offset: 0x00013C68
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.TriggeredAnyhow)
			{
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
