using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class IdolTshirt : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnded));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			base.GameRun.GainMaxHp(base.Counter, true, true);
			yield return new ApplyStatusEffectAction<LockedOn>(base.Battle.Player, new int?(base.Counter), default(int?), default(int?), default(int?), 0f, false);
			yield break;
		}
		private void OnPlayerTurnEnded(GameEventArgs args)
		{
			base.Blackout = true;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		public override string Name
		{
			get
			{
				if (base.Counter != 3)
				{
					return base.Name;
				}
				return base.ExtraDescription;
			}
		}
		[UsedImplicitly]
		public void SetToLarge()
		{
			base.Counter = 3;
		}
	}
}
