using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Koishi
{
	[UsedImplicitly]
	public sealed class MoodPeace : Mood
	{
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
		protected override void OnRemoving(Unit unit)
		{
			BattleController battle = base.Battle;
			if (battle != null && !battle.BattleShouldEnd)
			{
				this.React(new GainManaAction(this.Mana));
			}
		}
		public override string UnitEffectName
		{
			get
			{
				return "ChaowoLoop";
			}
		}
	}
}
