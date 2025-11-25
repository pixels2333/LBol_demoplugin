using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class ReimuEvilTerminatorSe : StatusEffect
	{
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Philosophies(base.Level);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new GainManaAction(this.Mana);
			yield break;
		}
	}
}
