using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000334 RID: 820
	[UsedImplicitly]
	public sealed class MystiaSing : Card
	{
		// Token: 0x06000BFB RID: 3067 RVA: 0x00017A1A File Offset: 0x00015C1A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (BattleAction battleAction in base.DebuffAction<TempFirepowerNegative>(selector.GetUnits(base.Battle), base.Value1, 0, 0, 0, true, 0.2f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new GainPowerAction(base.Value1);
			yield return new AddCardsToHandAction(Library.CreateCards<Shadow>(base.Value2, false), AddCardsType.Normal);
			yield break;
			yield break;
		}
	}
}
