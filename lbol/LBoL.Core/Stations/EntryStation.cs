using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;
namespace LBoL.Core.Stations
{
	public sealed class EntryStation : Station
	{
		public override StationType Type
		{
			get
			{
				return StationType.Entry;
			}
		}
		public Adventure DebutAdventure
		{
			get
			{
				if (this._adventure != null)
				{
					return this._adventure;
				}
				Type debutAdventureType = base.Stage.DebutAdventureType;
				if (debutAdventureType == null)
				{
					return null;
				}
				this._adventure = Library.CreateAdventure(debutAdventureType);
				this._adventure.SetStation(this);
				return this._adventure;
			}
		}
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}
		private Adventure _adventure;
	}
}
