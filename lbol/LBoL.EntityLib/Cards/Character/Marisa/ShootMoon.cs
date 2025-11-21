using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000446 RID: 1094
	[UsedImplicitly]
	public sealed class ShootMoon : Card
	{
		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06000EE7 RID: 3815 RVA: 0x0001B0F5 File Offset: 0x000192F5
		[UsedImplicitly]
		public int AttackTimes
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x06000EE8 RID: 3816 RVA: 0x0001B0F8 File Offset: 0x000192F8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, this.AttackTimes, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Overdrive(base.Value2))
			{
				yield return base.OverdriveAction(base.Value2);
				yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
