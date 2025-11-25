using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Adventure
{
	[UsedImplicitly]
	public sealed class RabbitFoot : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(this.OnBattleEnding));
		}
		private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
		{
			switch (base.Zone)
			{
			case CardZone.Draw:
				yield return new GainMoneyAction(base.Value1, SpecialSourceType.DrawZone);
				break;
			case CardZone.Hand:
				yield return new GainMoneyAction(base.Value1, SpecialSourceType.None);
				break;
			case CardZone.Discard:
				yield return new GainMoneyAction(base.Value1, SpecialSourceType.DisCardZone);
				break;
			}
			yield break;
		}
	}
}
