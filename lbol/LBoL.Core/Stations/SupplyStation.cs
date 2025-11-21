using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;
using UnityEngine;

namespace LBoL.Core.Stations
{
	// Token: 0x020000CC RID: 204
	public sealed class SupplyStation : Station, IAdventureStation
	{
		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x060008C2 RID: 2242 RVA: 0x000199D5 File Offset: 0x00017BD5
		public override StationType Type
		{
			get
			{
				return StationType.Supply;
			}
		}

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x060008C3 RID: 2243 RVA: 0x000199D8 File Offset: 0x00017BD8
		// (set) Token: 0x060008C4 RID: 2244 RVA: 0x000199E0 File Offset: 0x00017BE0
		public Adventure Adventure { get; private set; }

		// Token: 0x060008C5 RID: 2245 RVA: 0x000199EC File Offset: 0x00017BEC
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

		// Token: 0x060008C6 RID: 2246 RVA: 0x00019A40 File Offset: 0x00017C40
		public void Restore(Adventure adventure)
		{
			this.Adventure = adventure;
			this.Adventure.SetStation(this);
		}

		// Token: 0x060008C7 RID: 2247 RVA: 0x00019A55 File Offset: 0x00017C55
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
