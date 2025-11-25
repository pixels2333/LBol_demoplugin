using System;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class TempControlNegative : StatusEffect, IOpposing<TempControl>
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DollValueArgs>(base.Owner.DollValueGenerating, delegate(DollValueArgs args)
			{
				args.Value -= this.Level;
				args.AddModifier(this);
			});
			if (unit is PlayerUnit)
			{
				base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
				{
					this.React(new RemoveStatusEffectAction(this, true, 0.1f));
				});
				return;
			}
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
			{
				if (unit.HasStatusEffect<ExtraTurn>())
				{
					this.React(new RemoveStatusEffectAction(this, true, 0.1f));
				}
			});
			base.HandleOwnerEvent<GameEventArgs>(base.Battle.RoundEnded, delegate(GameEventArgs _)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			});
		}
		public OpposeResult Oppose(TempControl other)
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
