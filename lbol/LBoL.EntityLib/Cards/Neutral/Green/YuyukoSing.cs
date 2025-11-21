using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x02000304 RID: 772
	[UsedImplicitly]
	public sealed class YuyukoSing : Card
	{
		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000B7B RID: 2939 RVA: 0x00017088 File Offset: 0x00015288
		public override bool CanUpgrade
		{
			get
			{
				int? upgradeCounter = base.UpgradeCounter;
				int num = 99;
				return (upgradeCounter.GetValueOrDefault() < num) & (upgradeCounter != null);
			}
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000B7C RID: 2940 RVA: 0x000170B4 File Offset: 0x000152B4
		public override bool IsUpgraded
		{
			get
			{
				return base.UpgradeCounter > 0;
			}
		}

		// Token: 0x06000B7D RID: 2941 RVA: 0x000170DD File Offset: 0x000152DD
		public override void Initialize()
		{
			base.Initialize();
			base.UpgradeCounter = new int?(0);
		}

		// Token: 0x06000B7E RID: 2942 RVA: 0x000170F4 File Offset: 0x000152F4
		public override void Upgrade()
		{
			int? num = base.UpgradeCounter + 1;
			base.UpgradeCounter = num;
			base.ProcessKeywordUpgrade();
			base.CostChangeInUpgrading();
			this.NotifyChanged();
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000B7F RID: 2943 RVA: 0x00017144 File Offset: 0x00015344
		protected override int AdditionalDamage
		{
			get
			{
				if (!(base.UpgradeCounter > 0))
				{
					return 0;
				}
				return (base.UpgradeCounter.Value + 5) * base.UpgradeCounter.Value;
			}
		}

		// Token: 0x06000B80 RID: 2944 RVA: 0x0001718C File Offset: 0x0001538C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = ((base.UpgradeCounter > 0) ? base.UpgradeCounter.Value : 0);
			string text = "蝶之羽风" + Math.Min(num, 4).ToString();
			yield return base.AttackAction(selector, text);
			yield break;
		}
	}
}
