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
	[UsedImplicitly]
	public sealed class AsteroidBelt : Card
	{
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.BattleCardUsageHistory.Count != 0 && Enumerable.Contains<ManaColor>(Enumerable.Last<Card>(base.Battle.BattleCardUsageHistory).Config.Colors, ManaColor.Red);
			}
		}
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
