using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	public sealed class SeeFengshuiSe : StatusEffect
	{
		[UsedImplicitly]
		public ScryInfo Scry
		{
			get
			{
				return new ScryInfo(base.Level);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
			base.ReactOwnerEvent<CardMovingEventArgs>(base.Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(this.OnCardMoved));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ScryAction(this.Scry);
			yield break;
		}
		private IEnumerable<BattleAction> OnCardMoved(CardMovingEventArgs args)
		{
			if (args.ActionSource == this && args.Card.CardType == CardType.Defense)
			{
				yield return new CastBlockShieldAction(base.Battle.Player, new BlockInfo(this.Block, BlockShieldType.Direct), false);
			}
			yield break;
		}
		[UsedImplicitly]
		public int Block = 5;
	}
}
