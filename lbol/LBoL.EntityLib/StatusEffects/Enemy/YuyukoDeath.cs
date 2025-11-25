using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class YuyukoDeath : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
			base.HandleOwnerEvent<HealEventArgs>(base.Owner.HealingReceiving, new GameEventHandler<HealEventArgs>(this.OnOwnerHealing));
		}
		private IEnumerable<BattleAction> OnOwnerTurnEnded(GameEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd)
			{
				base.NotifyActivating();
				yield return PerformAction.Effect(base.Owner, "YuyukoDeathHit", 0f, "YuyukoDeathHit", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose((float)base.Level, false), "Instant", GunType.Single);
			}
			yield break;
		}
		private void OnOwnerHealing(HealEventArgs args)
		{
			base.NotifyActivating();
			args.CancelBy(this);
		}
		public override string UnitEffectName
		{
			get
			{
				return "YuyukoDeath";
			}
		}
	}
}
