using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002AF RID: 687
	[UsedImplicitly]
	public sealed class ShuimuBlock : Card
	{
		// Token: 0x06000A9B RID: 2715 RVA: 0x00015E48 File Offset: 0x00014048
		public override void Initialize()
		{
			base.Initialize();
			if (base.Config.Type == CardType.Tool)
			{
				throw new InvalidOperationException("Deck counter enabled card 'ShuimuBlock' must not be Tool.");
			}
			base.DeckCounter = new int?(0);
		}

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000A9C RID: 2716 RVA: 0x00015E78 File Offset: 0x00014078
		protected override int AdditionalBlock
		{
			get
			{
				return base.DeckCounter.Value;
			}
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x00015E93 File Offset: 0x00014093
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield break;
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x00015EA4 File Offset: 0x000140A4
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
