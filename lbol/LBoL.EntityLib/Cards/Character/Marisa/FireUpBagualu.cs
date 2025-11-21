using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200041F RID: 1055
	[UsedImplicitly]
	public sealed class FireUpBagualu : Card
	{
		// Token: 0x06000E82 RID: 3714 RVA: 0x0001A99B File Offset: 0x00018B9B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Charging>(base.Value1, 0, 0, 0, 0.2f);
			Burst statusEffect = base.Battle.Player.GetStatusEffect<Burst>();
			yield return base.BuffAction<Firepower>((statusEffect != null) ? (statusEffect.DamageRate * base.Value2) : base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
