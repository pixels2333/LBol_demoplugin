using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200043E RID: 1086
	[UsedImplicitly]
	public sealed class RedLaser : Card
	{
		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06000ED2 RID: 3794 RVA: 0x0001AF94 File Offset: 0x00019194
		[UsedImplicitly]
		public int AttackTimes
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000ED3 RID: 3795 RVA: 0x0001AF98 File Offset: 0x00019198
		[UsedImplicitly]
		public DamageInfo UpgradeDamage
		{
			get
			{
				if (base.Config.UpgradedDamage != null)
				{
					return DamageInfo.Attack((float)(base.Config.UpgradedDamage.Value + this.AdditionalDamage + base.DeltaDamage), base.IsAccuracy);
				}
				return this.Damage;
			}
		}

		// Token: 0x06000ED4 RID: 3796 RVA: 0x0001AFEE File Offset: 0x000191EE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (this.IsUpgraded)
			{
				base.CardGuns = new Guns(base.GunName, this.AttackTimes, true);
				foreach (GunPair gunPair in base.CardGuns.GunPairs)
				{
					yield return base.AttackAction(selector, gunPair);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
				EnemyUnit selectedEnemy = selector.SelectedEnemy;
				if (selectedEnemy.IsAlive)
				{
					yield return base.DebuffAction<Vulnerable>(selectedEnemy, 0, base.Value1, 0, 0, true, 0.2f);
				}
			}
			else if (base.Overdrive(base.Value2))
			{
				yield return base.OverdriveAction(base.Value2);
				base.CardGuns = new Guns(base.GunName, this.AttackTimes, true);
				foreach (GunPair gunPair2 in base.CardGuns.GunPairs)
				{
					GunPair gunPair3 = gunPair2;
					yield return base.AttackAction(selector, this.UpgradeDamage, gunPair3);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
				EnemyUnit selectedEnemy2 = selector.SelectedEnemy;
				if (selectedEnemy2.IsAlive)
				{
					yield return base.DebuffAction<Vulnerable>(selectedEnemy2, 0, base.Value1, 0, 0, true, 0.2f);
				}
			}
			else
			{
				base.CardGuns = new Guns(base.GunName);
				yield return base.AttackAction(selector, null);
			}
			yield break;
			yield break;
		}
	}
}
