using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
namespace LBoL.Core.Cards
{
	public abstract class ManaFreezer : Card
	{
		public virtual int FreezeLevel
		{
			get
			{
				return 1;
			}
		}
		public virtual int InitialTimes
		{
			get
			{
				return 1;
			}
		}
		public int FreezeTimes
		{
			get
			{
				return this._freezeTimes;
			}
			set
			{
				this._freezeTimes = value;
				this.NotifyChanged();
			}
		}
		public override void Initialize()
		{
			base.Initialize();
			this.FreezeTimes = this.InitialTimes;
		}
		public override bool OnDrawVisual
		{
			get
			{
				return false;
			}
		}
		public override bool OnDiscardVisual
		{
			get
			{
				return false;
			}
		}
		public override bool OnExileVisual
		{
			get
			{
				return false;
			}
		}
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}
		public override IEnumerable<BattleAction> OnDraw()
		{
			this.EnterHand();
			return null;
		}
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (srcZone == CardZone.Hand)
			{
				this.LeaveHand();
			}
			if (dstZone == CardZone.Hand)
			{
				this.EnterHand();
			}
			return null;
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				this.EnterHand();
			}
		}
		protected override void OnLeaveBattle()
		{
			base.Battle.CheckManaFreeze();
		}
		public override IEnumerable<BattleAction> OnDiscard(CardZone srcZone)
		{
			if (srcZone == CardZone.Hand)
			{
				this.LeaveHand();
			}
			return null;
		}
		public override IEnumerable<BattleAction> OnExile(CardZone srcZone)
		{
			if (srcZone == CardZone.Hand)
			{
				this.LeaveHand();
			}
			this.FreezeTimes = this.InitialTimes;
			return null;
		}
		private void EnterHand()
		{
			base.Battle.CheckManaFreeze();
		}
		private void LeaveHand()
		{
			base.Battle.CheckManaFreeze();
		}
		private int _freezeTimes;
	}
}
