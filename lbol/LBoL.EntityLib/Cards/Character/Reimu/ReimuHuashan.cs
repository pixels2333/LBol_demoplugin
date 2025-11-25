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
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class ReimuHuashan : Card
	{
		public override void Initialize()
		{
			base.Initialize();
			if (base.Config.Type == CardType.Tool)
			{
				throw new InvalidOperationException("Deck counter enabled card 'ReimuHuashan' must not be Tool.");
			}
			base.DeckCounter = new int?(0);
		}
		protected override string GetBaseDescription()
		{
			int? deckCounter = base.DeckCounter;
			int num = 0;
			if (!((deckCounter.GetValueOrDefault() == num) & (deckCounter != null)))
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			base.DeckCounter = new int?(1);
			Card deckCardByInstanceId = base.Battle.GameRun.GetDeckCardByInstanceId(base.InstanceId);
			if (deckCardByInstanceId != null)
			{
				deckCardByInstanceId.DeckCounter = base.DeckCounter;
			}
			return base.AfterUseAction();
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Battle.ReimuHuashanTimes > 0)
			{
				yield break;
			}
			if (this == Enumerable.FirstOrDefault<Card>(base.Battle.EnumerateAllCards(), delegate(Card card)
			{
				if (card is ReimuHuashan)
				{
					int? deckCounter = card.DeckCounter;
					int num2 = 1;
					return (deckCounter.GetValueOrDefault() == num2) & (deckCounter != null);
				}
				return false;
			}))
			{
				yield return new ExileCardAction(this);
				yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
				BattleController battle = base.Battle;
				int num = battle.ReimuHuashanTimes + 1;
				battle.ReimuHuashanTimes = num;
				base.DeckCounter = new int?(0);
				Card deckCardByInstanceId = base.Battle.GameRun.GetDeckCardByInstanceId(base.InstanceId);
				if (deckCardByInstanceId != null)
				{
					deckCardByInstanceId.DeckCounter = base.DeckCounter;
				}
			}
			yield break;
		}
	}
}
