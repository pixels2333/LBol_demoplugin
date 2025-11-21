using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000433 RID: 1075
	[UsedImplicitly]
	public sealed class NightSeeStar : Card
	{
		// Token: 0x06000EAE RID: 3758 RVA: 0x0001ACEA File Offset: 0x00018EEA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
