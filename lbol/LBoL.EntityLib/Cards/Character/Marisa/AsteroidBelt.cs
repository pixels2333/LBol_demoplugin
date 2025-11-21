using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200040E RID: 1038
	[UsedImplicitly]
	public sealed class AsteroidBelt : Card
	{
		// Token: 0x17000195 RID: 405
		// (get) Token: 0x06000E53 RID: 3667 RVA: 0x0001A5DD File Offset: 0x000187DD
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.BattleCardUsageHistory.Count != 0 && Enumerable.Contains<ManaColor>(Enumerable.Last<Card>(base.Battle.BattleCardUsageHistory).Config.Colors, ManaColor.Red);
			}
		}

		// Token: 0x06000E54 RID: 3668 RVA: 0x0001A61D File Offset: 0x0001881D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, false);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			if (base.TriggeredAnyhow)
			{
				yield return base.BuffAction<Charging>(base.Value2, 0, 0, 0, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
