using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x02000301 RID: 769
	[UsedImplicitly]
	public sealed class YouxiangAttack : Card
	{
		// Token: 0x06000B70 RID: 2928 RVA: 0x00016F8E File Offset: 0x0001518E
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}

		// Token: 0x06000B71 RID: 2929 RVA: 0x00016F94 File Offset: 0x00015194
		private DamageInfo CalculateDamage(ManaGroup? manaGroup)
		{
			if (manaGroup != null)
			{
				ManaGroup valueOrDefault = manaGroup.GetValueOrDefault();
				return DamageInfo.Attack((float)(base.RawDamage + (base.SynergyAmount(valueOrDefault, ManaColor.Any, 1) - 1) * base.Value1), valueOrDefault.Amount >= base.Value2);
			}
			return DamageInfo.Attack((float)base.RawDamage, base.IsAccuracy);
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000B72 RID: 2930 RVA: 0x00016FF6 File Offset: 0x000151F6
		[UsedImplicitly]
		public override DamageInfo Damage
		{
			get
			{
				return this.CalculateDamage(base.PendingManaUsage);
			}
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x00017004 File Offset: 0x00015204
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, this.CalculateDamage(new ManaGroup?(consumingMana)), null);
			yield break;
		}
	}
}
