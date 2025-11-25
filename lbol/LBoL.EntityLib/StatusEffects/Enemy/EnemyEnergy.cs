using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class EnemyEnergy : StatusEffect, IOpposing<EnemyEnergyNegative>
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<EnemyEnergy>(base.Owner, new int?(num), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
		public OpposeResult Oppose(EnemyEnergyNegative other)
		{
			base.Level = Math.Max(0, base.Level - other.Level);
			return OpposeResult.KeepSelf;
		}
	}
}
