using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Colorless
{
	// Token: 0x02000309 RID: 777
	[UsedImplicitly]
	public sealed class FengjiAttack : Card
	{
		// Token: 0x06000B8A RID: 2954 RVA: 0x0001720B File Offset: 0x0001540B
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}

		// Token: 0x06000B8B RID: 2955 RVA: 0x00017210 File Offset: 0x00015410
		private DamageInfo CalculateDamage(ManaGroup? manaGroup)
		{
			if (manaGroup != null)
			{
				ManaGroup valueOrDefault = manaGroup.GetValueOrDefault();
				return DamageInfo.Attack((float)(base.RawDamage + base.SynergyAmount(valueOrDefault, ManaColor.Colorless, 1) * base.Value1), base.IsAccuracy);
			}
			return DamageInfo.Attack((float)base.RawDamage, base.IsAccuracy);
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000B8C RID: 2956 RVA: 0x00017264 File Offset: 0x00015464
		[UsedImplicitly]
		public override DamageInfo Damage
		{
			get
			{
				return this.CalculateDamage(base.PendingManaUsage);
			}
		}

		// Token: 0x06000B8D RID: 2957 RVA: 0x00017272 File Offset: 0x00015472
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.CardGuns = new Guns(base.GunName, base.Value2, true);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, this.CalculateDamage(new ManaGroup?(consumingMana)), gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield break;
			yield break;
		}
	}
}
