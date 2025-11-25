using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;
namespace LBoL.Core.Stations
{
	public sealed class AdventureStation : Station, IAdventureStation
	{
		public override StationType Type
		{
			get
			{
				return StationType.Adventure;
			}
		}
		public Adventure Adventure { get; private set; }
		protected internal override void OnEnter()
		{
			this.Adventure = Library.CreateAdventure(base.Stage.GetAdventure());
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
				Adventure = this.Adventure.GetType().Name
			};
		}
	}
}
