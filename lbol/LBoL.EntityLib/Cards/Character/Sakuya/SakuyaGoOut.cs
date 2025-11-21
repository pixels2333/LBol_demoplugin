using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003AA RID: 938
	[UsedImplicitly]
	public sealed class SakuyaGoOut : Card
	{
		// Token: 0x1700017D RID: 381
		// (get) Token: 0x06000D54 RID: 3412 RVA: 0x00019390 File Offset: 0x00017590
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x00019393 File Offset: 0x00017593
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.HandZone.Count > base.Value1)
			{
				SelectHandInteraction interaction = new SelectHandInteraction(base.Value1, base.Value1, base.Battle.HandZone)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				yield return new DiscardManyAction(interaction.SelectedCards);
				interaction = null;
			}
			else
			{
				yield return new DiscardManyAction(base.Battle.HandZone);
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
