using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Others;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class QicaiLianhua : ShiningExhibit
	{
		protected override string GetBaseDescription()
		{
			if (!(this.ColorRecord != ManaGroup.Empty))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaGained, new EventSequencedReactor<ManaEventArgs>(this.OnManaGained));
		}
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			yield return new GainManaAction(manaGroup);
			yield break;
		}
		private IEnumerable<BattleAction> OnManaGained(ManaEventArgs args)
		{
			if (this.ColorRecord.Amount >= 7)
			{
				yield break;
			}
			foreach (ManaColor manaColor in ManaColors.WUBRGCP)
			{
				if (this.ColorRecord[manaColor] == 0 && args.Value.GetValue(manaColor) > 0)
				{
					this.ColorRecord[manaColor] = 1;
				}
			}
			if (base.Counter != this.ColorRecord.Amount)
			{
				base.Counter = this.ColorRecord.Amount;
			}
			if (this.ColorRecord.Amount >= 7)
			{
				base.NotifyActivating();
				if (base.Battle.HandIsFull)
				{
					yield return new AddCardsToDrawZoneAction(Library.CreateCards<MeilingLianhua>(1, false), DrawZoneTarget.Top, AddCardsType.Normal);
				}
				else
				{
					yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<MeilingLianhua>() });
				}
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			this.ColorRecord = ManaGroup.Empty;
		}
		[UsedImplicitly]
		public ManaGroup ColorRecord;
	}
}
