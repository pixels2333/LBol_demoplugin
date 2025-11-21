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
	// Token: 0x020003F2 RID: 1010
	[UsedImplicitly]
	public sealed class ReimuHuashan : Card
	{
		// Token: 0x06000E0C RID: 3596 RVA: 0x0001A0AA File Offset: 0x000182AA
		public override void Initialize()
		{
			base.Initialize();
			if (base.Config.Type == CardType.Tool)
			{
				throw new InvalidOperationException("Deck counter enabled card 'ReimuHuashan' must not be Tool.");
			}
			base.DeckCounter = new int?(0);
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x0001A0D8 File Offset: 0x000182D8
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

		// Token: 0x06000E0E RID: 3598 RVA: 0x0001A110 File Offset: 0x00018310
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

		// Token: 0x06000E0F RID: 3599 RVA: 0x0001A155 File Offset: 0x00018355
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x06000E10 RID: 3600 RVA: 0x0001A174 File Offset: 0x00018374
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
