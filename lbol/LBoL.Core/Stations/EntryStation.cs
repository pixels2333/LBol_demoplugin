using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C0 RID: 192
	public sealed class EntryStation : Station
	{
		// Token: 0x170002AC RID: 684
		// (get) Token: 0x0600085D RID: 2141 RVA: 0x0001892E File Offset: 0x00016B2E
		public override StationType Type
		{
			get
			{
				return StationType.Entry;
			}
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x0600085E RID: 2142 RVA: 0x00018934 File Offset: 0x00016B34
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

		// Token: 0x0600085F RID: 2143 RVA: 0x0001897F File Offset: 0x00016B7F
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}

		// Token: 0x04000396 RID: 918
		private Adventure _adventure;
	}
}
