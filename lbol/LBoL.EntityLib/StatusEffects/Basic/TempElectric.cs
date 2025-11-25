using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Basic
{
	[UsedImplicitly]
	public sealed class TempElectric : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.Source != base.Owner && args.Source.IsAlive && args.DamageInfo.DamageType == DamageType.Attack && args.DamageInfo.Amount > 0f)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, args.Source, DamageInfo.Reaction((float)base.Level, false), "电击", GunType.Single);
			}
			yield break;
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (!base.Owner.IsExtraTurn)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
		public override string UnitEffectName
		{
			get
			{
				return "ElectricLoop";
			}
		}
	}
}
