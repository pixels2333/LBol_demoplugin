using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003BA RID: 954
	[UsedImplicitly]
	public sealed class SilverBullet : Card
	{
		// Token: 0x17000180 RID: 384
		// (get) Token: 0x06000D7B RID: 3451 RVA: 0x000195A8 File Offset: 0x000177A8
		[UsedImplicitly]
		public int KnifeCount
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Count<Card>(base.Battle.EnumerateAllCardsButExile(), (Card card) => card is Knife);
				}
				return 0;
			}
		}

		// Token: 0x17000181 RID: 385
		// (get) Token: 0x06000D7C RID: 3452 RVA: 0x000195E3 File Offset: 0x000177E3
		protected override int AdditionalBlock
		{
			get
			{
				return base.Value1 * this.KnifeCount;
			}
		}

		// Token: 0x06000D7D RID: 3453 RVA: 0x000195F2 File Offset: 0x000177F2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.DefenseAction(false);
			yield break;
		}
	}
}
