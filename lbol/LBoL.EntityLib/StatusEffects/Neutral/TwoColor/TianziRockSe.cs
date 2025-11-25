using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class TianziRockSe : StatusEffect
	{
		protected override IEnumerable<string> RelativeEffects
		{
			get
			{
				if (base.Limit != 1)
				{
					return null;
				}
				return new string[] { "LockedOn" };
			}
		}
		protected override string GetBaseDescription()
		{
			if (base.Limit != 1)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(unit.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = damageInfo.ReduceBy(base.Level);
				args.AddModifier(this);
			}
		}
		private IEnumerable<BattleAction> OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (base.Limit == 1 && args.Effect is LockedOn)
			{
				args.CancelBy(this);
				base.NotifyActivating();
				yield return PerformAction.Sfx("Amulet", 0f);
				yield return PerformAction.SePop(base.Owner, args.Effect.Name);
			}
			yield break;
		}
	}
}
