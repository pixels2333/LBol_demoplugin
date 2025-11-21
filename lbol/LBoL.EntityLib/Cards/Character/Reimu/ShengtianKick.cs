using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003FB RID: 1019
	[UsedImplicitly]
	public sealed class ShengtianKick : Card
	{
		// Token: 0x06000E25 RID: 3621 RVA: 0x0001A2C1 File Offset: 0x000184C1
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			yield return new ApplyStatusEffectAction<TempFirepower>(base.Battle.Player, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
