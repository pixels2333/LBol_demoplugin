using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;
using UnityEngine;
namespace LBoL.Core.Stations
{
	public sealed class TradeStation : Station, IAdventureStation
	{
		public override StationType Type
		{
			get
			{
				return StationType.Trade;
			}
		}
		public Adventure Adventure { get; private set; }
		protected internal override void OnEnter()
		{
			Type tradeAdventureType = base.Stage.TradeAdventureType;
			if (tradeAdventureType == null)
			{
				Debug.LogError("Stage " + base.Stage.Id + " has no trade adventure");
				return;
			}
			this.Adventure = Library.CreateAdventure(tradeAdventureType);
			this.Adventure.SetStation(this);
		}
		public void Restore(Adventure adventure)
		{
			this.Adventure = adventure;
			this.Adventure.SetStation(this);
		}
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type,
				Adventure = this.Adventure.Id
			};
		}
	}
}
