using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.GapOptions;
using LBoL.Core.Stations;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3)]
	public sealed class Baota : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Counter > 0)
			{
				base.NotifyActivating();
				ManaGroup manaGroup = base.Mana * base.Counter;
				yield return new GainManaAction(manaGroup);
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Counter), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.GapOptionsGenerating, delegate(StationEventArgs args)
			{
				if (base.Counter < base.Value1)
				{
					((GapStation)args.Station).GapOptions.Add(Library.CreateGapOption<UpgradeBaota>());
				}
			});
		}
		public void GapOption()
		{
			int num = base.Counter + 1;
			base.Counter = num;
			base.NotifyActivating();
		}
	}
}
