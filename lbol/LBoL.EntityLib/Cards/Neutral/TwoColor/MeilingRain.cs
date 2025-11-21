using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200029B RID: 667
	[UsedImplicitly]
	public sealed class MeilingRain : Card
	{
		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000A67 RID: 2663 RVA: 0x00015AA0 File Offset: 0x00013CA0
		[UsedImplicitly]
		public int AttackTimes
		{
			get
			{
				ManaGroup? pendingManaUsage = base.PendingManaUsage;
				if (pendingManaUsage != null)
				{
					ManaGroup valueOrDefault = pendingManaUsage.GetValueOrDefault();
					return base.SynergyAmount(valueOrDefault, ManaColor.Philosophy, 1) + 1;
				}
				return 1;
			}
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x00015AD4 File Offset: 0x00013CD4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, this.AttackTimes, true);
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
