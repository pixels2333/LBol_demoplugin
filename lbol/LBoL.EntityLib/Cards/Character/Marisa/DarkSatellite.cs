using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000418 RID: 1048
	[UsedImplicitly]
	public sealed class DarkSatellite : Card
	{
		// Token: 0x17000198 RID: 408
		// (get) Token: 0x06000E6A RID: 3690 RVA: 0x0001A763 File Offset: 0x00018963
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.BattleCardUsageHistory.Count != 0 && Enumerable.Contains<ManaColor>(Enumerable.Last<Card>(base.Battle.BattleCardUsageHistory).Config.Colors, ManaColor.Black);
			}
		}

		// Token: 0x06000E6B RID: 3691 RVA: 0x0001A7A3 File Offset: 0x000189A3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.TriggeredAnyhow)
			{
				yield return new DrawManyCardAction(base.Value1);
			}
			yield break;
		}
	}
}
