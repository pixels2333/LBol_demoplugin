using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003A8 RID: 936
	[UsedImplicitly]
	public sealed class SakuyaDraw : Card
	{
		// Token: 0x06000D4E RID: 3406 RVA: 0x000192FD File Offset: 0x000174FD
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			base.Battle.DrawAfterDiscard += base.Value1;
			return base.OnDiscard(srcZone);
		}

		// Token: 0x06000D4F RID: 3407 RVA: 0x0001931E File Offset: 0x0001751E
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			base.Battle.DrawAfterDiscard += base.Value1;
			return base.OnExile(srcZone);
		}
	}
}
