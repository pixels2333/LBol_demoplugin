using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;
using UnityEngine;
namespace LBoL.Core.Stations
{
	public sealed class SupplyStation : Station, IAdventureStation
	{
		public override StationType Type
		{
			get
			{
				return StationType.Supply;
			}
		}
		public Adventure Adventure { get; private set; }
		protected internal override void OnEnter()
		{
			Type supplyAdventureType = base.Stage.SupplyAdventureType;
			if (supplyAdventureType == null)
			{
				Debug.LogError("Stage " + base.Stage.Id + " has no supply adventure");
				return;
			}
			this.Adventure = Library.CreateAdventure(supplyAdventureType);
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
				Type = StationType.Supply,
				Adventure = this.Adventure.GetType().Name
			};
		}
	}
}
