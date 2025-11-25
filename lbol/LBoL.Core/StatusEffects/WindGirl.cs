using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class WindGirl : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			this.React(new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
		}
		private IEnumerable<BattleAction> OnOwnerTurnEnding(GameEventArgs args)
		{
			Graze statusEffect = base.Owner.GetStatusEffect<Graze>();
			int num = ((statusEffect != null) ? statusEffect.Level : 0);
			if (num < base.Level)
			{
				yield return new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Level - num), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				this.React(new ApplyStatusEffectAction<Graze>(base.Owner, new int?(other.Level), default(int?), default(int?), default(int?), 0f, true));
			}
			return flag;
		}
	}
}
