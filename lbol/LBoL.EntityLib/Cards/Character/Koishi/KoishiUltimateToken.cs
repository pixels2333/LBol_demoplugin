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
	// Token: 0x0200047E RID: 1150
	[UsedImplicitly]
	public sealed class KoishiUltimateToken : Card
	{
		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000F66 RID: 3942 RVA: 0x0001B932 File Offset: 0x00019B32
		[UsedImplicitly]
		public ManaGroup TotalMana
		{
			get
			{
				return base.Mana * base.Value2;
			}
		}

		// Token: 0x06000F67 RID: 3943 RVA: 0x0001B945 File Offset: 0x00019B45
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

		// Token: 0x06000F68 RID: 3944 RVA: 0x0001B95C File Offset: 0x00019B5C
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
