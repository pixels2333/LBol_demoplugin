using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class DoubleLianhuadieSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<BlockShieldEventArgs>(base.Battle.Player.BlockShieldGained, new EventSequencedReactor<BlockShieldEventArgs>(this.OnBlockShieldGained));
		}
		private IEnumerable<BattleAction> OnBlockShieldGained(BlockShieldEventArgs args)
		{
			EnemyUnit target = base.Battle.RandomAliveEnemy;
			if (target == null)
			{
				yield break;
			}
			if (args.Block > 0f || args.Shield > 0f)
			{
				yield return PerformAction.Effect(target, "DoubleLianhuadieHit", 0f, "DoubleLianhuadie", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new DamageAction(base.Battle.Player, target, DamageInfo.Reaction((float)base.Level, false), "Instant", GunType.Single);
			}
			yield break;
		}
	}
}
