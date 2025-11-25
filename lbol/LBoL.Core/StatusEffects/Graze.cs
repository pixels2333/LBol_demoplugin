using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Graze : StatusEffect
	{
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
		protected override string GetBaseDescription()
		{
			if (!(base.Owner is EnemyUnit))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
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
		public void BeenAccurate()
		{
			int num = base.Level - 1;
			base.Level = num;
			if (base.Level == 0)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			}
		}
		public override string UnitEffectName
		{
			get
			{
				return "GrazeLoop";
			}
		}
	}
}
