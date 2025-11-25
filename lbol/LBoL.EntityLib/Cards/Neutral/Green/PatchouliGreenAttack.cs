using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	[UsedImplicitly]
	public sealed class PatchouliGreenAttack : Card
	{
		public override bool Triggered
		{
			get
			{
				return this.CanUse;
			}
		}
		public override bool CanUse
		{
			get
			{
				return this.BattleAmount > this.BaseAmount;
			}
		}
		[UsedImplicitly]
		public int BattleAmount
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return base.Battle.BattleMana.Amount;
			}
		}
		[UsedImplicitly]
		public int BaseAmount
		{
			get
			{
				return base.GameRun.BaseMana.Amount;
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<ManaEventArgs>(battle.ManaGained, delegate(ManaEventArgs _)
			{
				this.NotifyChanged();
			}, (GameEventPriority)0);
			base.HandleBattleEvent<ManaEventArgs>(battle.ManaLost, delegate(ManaEventArgs _)
			{
				this.NotifyChanged();
			}, (GameEventPriority)0);
		}
	}
}
