using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class ShuimuBlock : Card
	{
		public override void Initialize()
		{
			base.Initialize();
			if (base.Config.Type == CardType.Tool)
			{
				throw new InvalidOperationException("Deck counter enabled card 'ShuimuBlock' must not be Tool.");
			}
			base.DeckCounter = new int?(0);
		}
		protected override int AdditionalBlock
		{
			get
			{
				return base.DeckCounter.Value;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield break;
		}
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			base.DeckCounter += base.Value1;
			Card deckCardByInstanceId = base.Battle.GameRun.GetDeckCardByInstanceId(base.InstanceId);
			if (deckCardByInstanceId != null)
			{
				deckCardByInstanceId.DeckCounter = base.DeckCounter;
			}
			return base.AfterUseAction();
		}
	}
}
