using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000336 RID: 822
	[UsedImplicitly]
	public sealed class PoorAttack : Card
	{
		// Token: 0x06000BFF RID: 3071 RVA: 0x00017A58 File Offset: 0x00015C58
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.GameRun.LoseMoney(base.Value1);
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
