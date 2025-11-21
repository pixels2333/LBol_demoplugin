using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x0200009C RID: 156
	[UsedImplicitly]
	public sealed class Graze : StatusEffect
	{
		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06000772 RID: 1906 RVA: 0x0001605E File Offset: 0x0001425E
		protected override Keyword Keywords
		{
			get
			{
				if (!(base.Owner is EnemyUnit))
				{
					return base.Config.Keywords;
				}
				return Keyword.None;
			}
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x0001607B File Offset: 0x0001427B
		protected override string GetBaseDescription()
		{
			if (!(base.Owner is EnemyUnit))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x00016098 File Offset: 0x00014298
		public void LoseGraze()
		{
			if (base.IsAutoDecreasing)
			{
				int num = base.Level - 1;
				base.Level = num;
				if (base.Level == 0)
				{
					this.React(new RemoveStatusEffectAction(this, true, 0.1f));
					return;
				}
			}
			else
			{
				base.IsAutoDecreasing = true;
			}
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x000160E4 File Offset: 0x000142E4
		public void Activate()
		{
			int num = base.Level - 1;
			base.Level = num;
			if (base.Level > 0)
			{
				base.NotifyActivating();
				return;
			}
			this.React(new RemoveStatusEffectAction(this, true, 0.1f));
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00016128 File Offset: 0x00014328
		public void BeenAccurate()
		{
			int num = base.Level - 1;
			base.Level = num;
			if (base.Level == 0)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			}
		}

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06000777 RID: 1911 RVA: 0x00016164 File Offset: 0x00014364
		public override string UnitEffectName
		{
			get
			{
				return "GrazeLoop";
			}
		}
	}
}
