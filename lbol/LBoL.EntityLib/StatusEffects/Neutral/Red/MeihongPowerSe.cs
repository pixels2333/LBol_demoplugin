using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.Red
{
	public sealed class MeihongPowerSe : StatusEffect
	{
		[UsedImplicitly]
		public int Heal
		{
			get
			{
				return base.Level * 2;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarting));
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(this.OnBattleEnding));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
		{
			if (base.Battle.Player.IsAlive && base.Battle.Player.Hp <= (base.Battle.Player.MaxHp + 1) / 2)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Battle.Player, base.Battle.Player, this.Heal, HealType.Normal, 0.2f);
			}
			yield break;
		}
	}
}
