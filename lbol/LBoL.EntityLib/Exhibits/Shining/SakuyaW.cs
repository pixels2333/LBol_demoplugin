using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Character.Sakuya;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class SakuyaW : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.ReactBattleEvent<GameEventArgs>(base.Battle.Reshuffled, new EventSequencedReactor<GameEventArgs>(this.OnReshuffled));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
		private IEnumerable<BattleAction> OnReshuffled(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Value1, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
