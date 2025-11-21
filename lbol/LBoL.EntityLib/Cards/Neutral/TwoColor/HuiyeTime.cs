using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn.Partners;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000292 RID: 658
	[UsedImplicitly]
	public sealed class HuiyeTime : Card
	{
		// Token: 0x06000A4F RID: 2639 RVA: 0x000158B0 File Offset: 0x00013AB0
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this && !hand.IsTempRetain && !hand.IsRetain && !hand.Summoned));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, base.Value1, list);
			}
			return null;
		}

		// Token: 0x06000A50 RID: 2640 RVA: 0x000158F6 File Offset: 0x00013AF6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			IReadOnlyList<Card> readOnlyList = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards : null);
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				foreach (Card card in readOnlyList)
				{
					if (!card.IsRetain && !card.Summoned)
					{
						card.IsTempRetain = true;
					}
				}
			}
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<HuiyeTimeSe>(0, 0, base.Value1, 0, 0.2f);
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
