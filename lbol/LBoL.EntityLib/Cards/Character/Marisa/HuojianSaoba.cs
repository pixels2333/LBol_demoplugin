using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000421 RID: 1057
	[UsedImplicitly]
	public sealed class HuojianSaoba : Card
	{
		// Token: 0x06000E86 RID: 3718 RVA: 0x0001A9CB File Offset: 0x00018BCB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ApplyStatusEffectAction<Graze>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0.4f, true);
			yield return new ApplyStatusEffectAction<Charging>(base.Battle.Player, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
