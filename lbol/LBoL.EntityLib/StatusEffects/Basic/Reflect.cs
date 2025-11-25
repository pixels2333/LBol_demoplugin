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
	public sealed class Reflect : StatusEffect
	{
		public string Gun { get; set; } = "Reflect";
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.Source != base.Owner && args.Source.IsAlive && args.DamageInfo.DamageType == DamageType.Attack)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, args.Source, DamageInfo.Reaction((float)base.Level, false), this.Gun, GunType.Single);
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
