using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class KoishiUltimateToken : Card
	{
		[UsedImplicitly]
		public ManaGroup TotalMana
		{
			get
			{
				return base.Mana * base.Value2;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			string text = "生命绽放" + Mathf.Clamp(base.Value2 - 1, 0, 5).ToString();
			yield return base.AttackAction(selector, new GunPair(text, GunType.Single));
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new GainManaAction(this.TotalMana);
			yield break;
		}
		public override IEnumerable<BattleAction> OnRetain()
		{
			if (base.Zone == CardZone.Hand)
			{
				base.DeltaDamage += base.Value1;
				int num = base.DeltaValue2 + 1;
				base.DeltaValue2 = num;
			}
			return null;
		}
	}
}
