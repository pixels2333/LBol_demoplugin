using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A6 RID: 678
	[UsedImplicitly]
	public sealed class SakuraBlade : Card
	{
		// Token: 0x06000A86 RID: 2694 RVA: 0x00015CE9 File Offset: 0x00013EE9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield return base.DebuffAction<LockedOn>(selector.SelectedEnemy, base.Value1, 0, 0, 0, true, 0.2f);
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
			yield break;
		}
	}
}
