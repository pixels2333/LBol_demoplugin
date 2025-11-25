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
	public sealed class BattleAdvTestStation : Station
	{
		public override StationType Type
		{
			get
			{
				return StationType.BattleAdvTest;
			}
		}
		public BattleAdvTestStation()
		{
			base.Status = StationStatus.Unknown;
		}
		public void SetEnemy(EnemyGroupEntry entry)
		{
			if (this._enemy != null || this._adventureType != null)
			{
				throw new InvalidOperationException("BattleAdvTestStation already has enemy or adventure");
			}
			this._enemy = entry;
			this.EnemyGroup = entry.Generate(base.GameRun);
		}
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
		public EnemyGroup EnemyGroup { get; private set; }
		public Adventure Adventure { get; private set; }
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}
		private EnemyGroupEntry _enemy;
		private Type _adventureType;
	}
}
