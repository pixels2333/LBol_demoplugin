using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x0200032C RID: 812
	[UsedImplicitly]
	public sealed class BlackHorseZui : Card
	{
		// Token: 0x06000BE9 RID: 3049 RVA: 0x000178E3 File Offset: 0x00015AE3
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
