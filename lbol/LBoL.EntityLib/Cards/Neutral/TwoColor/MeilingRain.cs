using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class MeilingRain : Card
	{
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
