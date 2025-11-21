using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000277 RID: 631
	[UsedImplicitly]
	public sealed class NazrinBaota : Card
	{
		// Token: 0x06000A03 RID: 2563 RVA: 0x00015276 File Offset: 0x00013476
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.UpgradeRandomHandAction(base.Value1, CardType.Attack);
			yield break;
		}
	}
}
