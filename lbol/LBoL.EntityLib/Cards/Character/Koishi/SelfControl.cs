using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000492 RID: 1170
	[UsedImplicitly]
	public sealed class SelfControl : Card
	{
		// Token: 0x06000FA6 RID: 4006 RVA: 0x0001BE90 File Offset: 0x0001A090
		public override Interaction Precondition()
		{
			List<SelfControl> list = Enumerable.ToList<SelfControl>(Library.CreateCards<SelfControl>(2, this.IsUpgraded));
			SelfControl selfControl = list[0];
			SelfControl selfControl2 = list[1];
			selfControl.ChoiceCardIndicator = 1;
			selfControl2.ChoiceCardIndicator = 2;
			selfControl.SetBattle(base.Battle);
			selfControl2.SetBattle(base.Battle);
			return new MiniSelectCardInteraction(list, false, false, false);
		}

		// Token: 0x06000FA7 RID: 4007 RVA: 0x0001BEEA File Offset: 0x0001A0EA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			MiniSelectCardInteraction miniSelectCardInteraction = (MiniSelectCardInteraction)precondition;
			Card card = ((miniSelectCardInteraction != null) ? miniSelectCardInteraction.SelectedCard : null);
			if (card != null)
			{
				if (card.ChoiceCardIndicator == 1)
				{
					yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
				}
				else
				{
					yield return base.BuffAction<MoodPeace>(0, 0, 0, 0, 0.2f);
				}
			}
			yield return base.BuffAction<SelfControlSe>(base.Value1, 0, 0, base.Value2, 0.2f);
			yield break;
		}
	}
}
