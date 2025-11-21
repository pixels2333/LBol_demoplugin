using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000472 RID: 1138
	[UsedImplicitly]
	public sealed class KoishiFire : Card
	{
		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000F4A RID: 3914 RVA: 0x0001B746 File Offset: 0x00019946
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && (base.Battle.Player.HasStatusEffect<MoodPeace>() || base.Battle.Player.HasStatusEffect<MoodPassion>());
			}
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x0001B776 File Offset: 0x00019976
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.TriggeredAnyhow)
			{
				if (base.Battle.Player.HasStatusEffect<MoodPeace>())
				{
					yield return base.BuffAction<Spirit>(base.Value1, 0, 0, 0, 0.2f);
				}
				else
				{
					yield return base.BuffAction<Firepower>(base.Value1 + base.Value2, 0, 0, 0, 0.2f);
				}
			}
			else
			{
				yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
