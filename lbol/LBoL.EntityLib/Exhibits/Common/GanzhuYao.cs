using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class GanzhuYao : Exhibit
	{
		public override string OverrideIconName
		{
			get
			{
				if (base.Counter != 0)
				{
					return base.Id + "Inactive";
				}
				return base.Id;
			}
		}
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<DieEventArgs>(base.Owner.Dying, new GameEventHandler<DieEventArgs>(this.OnDying));
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		private void OnDying(DieEventArgs args)
		{
			if (base.Counter == 0)
			{
				base.NotifyActivating();
				int num = ((double)(args.Unit.MaxHp * base.Value1) / 100.0).RoundToInt();
				base.GameRun.SetHpAndMaxHp(num, num, false);
				args.CancelBy(this);
				base.Counter = 1;
				if (base.GameRun.Battle != null)
				{
					this.React(new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true));
					base.Blackout = true;
				}
			}
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				if (base.Counter == 1)
				{
					base.Blackout = true;
				}
			});
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Counter == 1)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
				base.Blackout = true;
			}
			yield break;
		}
	}
}
