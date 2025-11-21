using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004F8 RID: 1272
	[UsedImplicitly]
	public sealed class MeihongAttack : Card
	{
		// Token: 0x060010BB RID: 4283 RVA: 0x0001D34D File Offset: 0x0001B54D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value1);
			base.CardGuns = new Guns(base.GunName, base.Value2, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield break;
			yield break;
		}
	}
}
