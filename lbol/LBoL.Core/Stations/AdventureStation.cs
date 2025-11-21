using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;

namespace LBoL.Core.Stations
{
	// Token: 0x020000BA RID: 186
	public sealed class AdventureStation : Station, IAdventureStation
	{
		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06000831 RID: 2097 RVA: 0x00018495 File Offset: 0x00016695
		public override StationType Type
		{
			get
			{
				return StationType.Adventure;
			}
		}

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06000832 RID: 2098 RVA: 0x00018498 File Offset: 0x00016698
		// (set) Token: 0x06000833 RID: 2099 RVA: 0x000184A0 File Offset: 0x000166A0
		public Adventure Adventure { get; private set; }

		// Token: 0x06000834 RID: 2100 RVA: 0x000184A9 File Offset: 0x000166A9
		protected internal override void OnEnter()
		{
			this.Adventure = Library.CreateAdventure(base.Stage.GetAdventure());
			this.Adventure.SetStation(this);
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x000184CD File Offset: 0x000166CD
		public void Restore(Adventure adventure)
		{
			this.Adventure = adventure;
			this.Adventure.SetStation(this);
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x000184E2 File Offset: 0x000166E2
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
