using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.Units;

namespace LBoL.Core.Stations
{
	// Token: 0x020000BB RID: 187
	public sealed class BattleAdvTestStation : Station
	{
		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06000838 RID: 2104 RVA: 0x00018513 File Offset: 0x00016713
		public override StationType Type
		{
			get
			{
				return StationType.BattleAdvTest;
			}
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x00018517 File Offset: 0x00016717
		public BattleAdvTestStation()
		{
			base.Status = StationStatus.Unknown;
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x00018526 File Offset: 0x00016726
		public void SetEnemy(EnemyGroupEntry entry)
		{
			if (this._enemy != null || this._adventureType != null)
			{
				throw new InvalidOperationException("BattleAdvTestStation already has enemy or adventure");
			}
			this._enemy = entry;
			this.EnemyGroup = entry.Generate(base.GameRun);
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x00018564 File Offset: 0x00016764
		public void SetAdventure(Adventure adventure)
		{
			if (this._enemy != null || this._adventureType != null)
			{
				throw new InvalidOperationException("BattleAdvTestStation already has enemy or adventure");
			}
			this._adventureType = adventure.GetType();
			this.Adventure = adventure;
			this.Adventure.SetStation(this);
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x000185B4 File Offset: 0x000167B4
		public override void Finish()
		{
			Card card = null;
			Card card2 = null;
			Card card3 = null;
			Card card4 = null;
			List<Card> list = new List<Card>();
			foreach (ValueTuple<Type, CardConfig> valueTuple in Library.EnumerateCardTypes())
			{
				Type item = valueTuple.Item1;
				CardConfig item2 = valueTuple.Item2;
				if (card != null && card2 != null && card3 != null && card4 != null)
				{
					break;
				}
				if (card == null && item2.IsXCost)
				{
					list.Add(card = Library.CreateCard(item));
				}
				else if (card2 == null && item2.Type == CardType.Tool)
				{
					list.Add(card2 = Library.CreateCard(item));
				}
				else if (card3 == null && item2.Type == CardType.Ability)
				{
					list.Add(card3 = Library.CreateCard(item));
				}
				else if (card4 == null && item2.Type == CardType.Friend)
				{
					list.Add(card4 = Library.CreateCard(item));
				}
			}
			base.Rewards.Add(StationReward.CreateCards(list));
			base.Finish();
		}

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x0600083D RID: 2109 RVA: 0x000186C4 File Offset: 0x000168C4
		// (set) Token: 0x0600083E RID: 2110 RVA: 0x000186CC File Offset: 0x000168CC
		public EnemyGroup EnemyGroup { get; private set; }

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x0600083F RID: 2111 RVA: 0x000186D5 File Offset: 0x000168D5
		// (set) Token: 0x06000840 RID: 2112 RVA: 0x000186DD File Offset: 0x000168DD
		public Adventure Adventure { get; private set; }

		// Token: 0x06000841 RID: 2113 RVA: 0x000186E6 File Offset: 0x000168E6
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}

		// Token: 0x0400038E RID: 910
		private EnemyGroupEntry _enemy;

		// Token: 0x0400038F RID: 911
		private Type _adventureType;
	}
}
