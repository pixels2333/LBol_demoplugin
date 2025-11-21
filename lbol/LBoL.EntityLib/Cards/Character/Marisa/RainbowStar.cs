using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200043B RID: 1083
	[UsedImplicitly]
	public sealed class RainbowStar : Card
	{
		// Token: 0x06000EC9 RID: 3785 RVA: 0x0001AEC0 File Offset: 0x000190C0
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}

		// Token: 0x06000ECA RID: 3786 RVA: 0x0001AEC4 File Offset: 0x000190C4
		private DamageInfo CalculateDamage(ManaGroup? manaGroup)
		{
			if (manaGroup != null)
			{
				ManaGroup valueOrDefault = manaGroup.GetValueOrDefault();
				return DamageInfo.Attack((float)(base.RawDamage + base.SynergyAmount(valueOrDefault, ManaColor.Any, 1) * base.Value1 + base.SynergyAmount(valueOrDefault, ManaColor.Philosophy, 1) * base.Value2), base.IsAccuracy);
			}
			return DamageInfo.Attack((float)base.RawDamage, base.IsAccuracy);
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06000ECB RID: 3787 RVA: 0x0001AF29 File Offset: 0x00019129
		[UsedImplicitly]
		public override DamageInfo Damage
		{
			get
			{
				return this.CalculateDamage(base.PendingManaUsage);
			}
		}

		// Token: 0x06000ECC RID: 3788 RVA: 0x0001AF37 File Offset: 0x00019137
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, this.CalculateDamage(new ManaGroup?(consumingMana)), null);
			yield break;
		}
	}
}
