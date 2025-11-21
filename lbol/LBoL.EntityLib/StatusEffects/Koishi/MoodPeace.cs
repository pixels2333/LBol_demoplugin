using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x0200007E RID: 126
	[UsedImplicitly]
	public sealed class MoodPeace : Mood
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x000057BE File Offset: 0x000039BE
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				if (base.Owner == null || !base.Owner.HasStatusEffect<UpgradePeace>())
				{
					return ManaGroup.Philosophies(3);
				}
				return ManaGroup.Philosophies(4);
			}
		}

		// Token: 0x060001C1 RID: 449 RVA: 0x000057E4 File Offset: 0x000039E4
		protected override void OnRemoving(Unit unit)
		{
			BattleController battle = base.Battle;
			if (battle != null && !battle.BattleShouldEnd)
			{
				this.React(new GainManaAction(this.Mana));
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060001C2 RID: 450 RVA: 0x00005819 File Offset: 0x00003A19
		public override string UnitEffectName
		{
			get
			{
				return "ChaowoLoop";
			}
		}
	}
}
