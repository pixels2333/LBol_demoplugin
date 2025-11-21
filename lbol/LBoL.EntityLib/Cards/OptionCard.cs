using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards
{
	// Token: 0x0200025A RID: 602
	public abstract class OptionCard : Card
	{
		// Token: 0x060009C0 RID: 2496 RVA: 0x00014E73 File Offset: 0x00013073
		public virtual IEnumerable<BattleAction> TakeEffectActions()
		{
			yield break;
		}
	}
}
