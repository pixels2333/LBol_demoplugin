using System;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class TempControl : StatusEffect, IOpposing<TempControlNegative>
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DollValueArgs>(base.Owner.DollValueGenerating, delegate(DollValueArgs args)
			{
				args.Value += base.Level;
				args.AddModifier(this);
			});
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			});
		}
		public OpposeResult Oppose(TempControlNegative other)
		{
			if (base.Level < other.Level)
			{
				other.Level -= base.Level;
				return OpposeResult.KeepOther;
			}
			if (base.Level == other.Level)
			{
				return OpposeResult.Neutralize;
			}
			base.Level -= other.Level;
			return OpposeResult.KeepSelf;
		}
	}
}
