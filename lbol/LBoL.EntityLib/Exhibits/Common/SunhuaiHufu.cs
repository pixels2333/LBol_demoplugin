using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class SunhuaiHufu : Exhibit
	{
		protected override string GetBaseDescription()
		{
			if (base.GameRun == null || !Enumerable.Contains<Exhibit>(base.GameRun.Player.Exhibits, this))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnAdded(PlayerUnit player)
		{
			this.RefreshCounter();
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsAdded, delegate(CardsEventArgs _)
			{
				this.RefreshCounter();
			});
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsRemoved, delegate(CardsEventArgs _)
			{
				this.RefreshCounter();
			});
		}
		private void RefreshCounter()
		{
			if (base.GameRun != null)
			{
				base.Counter = Enumerable.Count<Card>(base.GameRun.BaseDeck, (Card card) => card.CardType == CardType.Misfortune);
			}
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Counter > 0)
			{
				int level = base.Counter * base.Value1;
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(level), default(int?), default(int?), default(int?), 0f, true);
				yield return new ApplyStatusEffectAction<Spirit>(base.Owner, new int?(level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
