using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004BA RID: 1210
	[UsedImplicitly]
	public sealed class FrozenOrb : Card
	{
		// Token: 0x0600100C RID: 4108 RVA: 0x0001C755 File Offset: 0x0001A955
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.03f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new AddCardsToHandAction(Library.CreateCards<IceBolt>(base.Value1, false), AddCardsType.Normal);
			yield break;
			yield break;
		}
	}
}
