using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class JingQiXi : Exhibit
	{
		private ManaGroup ManaLost { get; set; }
		[UsedImplicitly]
		public ManaGroup ManaGain
		{
			get
			{
				return this.ManaLost * 2;
			}
		}
		private ManaColor LostManaColor { get; set; }
		protected override void OnEnterBattle()
		{
			this.ManaLost = ManaGroup.Empty;
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
		protected override string GetBaseDescription()
		{
			if (this.ManaLost.Amount <= 0)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Owner.TurnCounter % 2 == 1)
			{
				base.NotifyActivating();
				if (base.Battle.BattleMana.Amount > 0)
				{
					this.LostManaColor = base.Battle.BattleMana.EnumerateComponents().Sample(base.GameRun.BattleRng);
					this.ManaLost = ManaGroup.Single(this.LostManaColor);
					yield return new LoseManaAction(this.ManaLost);
					this.NotifyChanged();
				}
			}
			else if (this.ManaLost.Amount > 0)
			{
				base.NotifyActivating();
				yield return new GainManaAction(this.ManaGain);
				this.ManaLost = ManaGroup.Empty;
				this.NotifyChanged();
			}
			yield break;
		}
	}
}
