using System;
using LBoL.Core.Adventures;
using LBoL.Core.SaveData;
using UnityEngine;

namespace LBoL.Core.Stations
{
	// Token: 0x020000CD RID: 205
	public sealed class TradeStation : Station, IAdventureStation
	{
		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x060008C9 RID: 2249 RVA: 0x00019A81 File Offset: 0x00017C81
		public override StationType Type
		{
			get
			{
				return StationType.Trade;
			}
		}

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x060008CA RID: 2250 RVA: 0x00019A85 File Offset: 0x00017C85
		// (set) Token: 0x060008CB RID: 2251 RVA: 0x00019A8D File Offset: 0x00017C8D
		public Adventure Adventure { get; private set; }

		// Token: 0x060008CC RID: 2252 RVA: 0x00019A98 File Offset: 0x00017C98
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

		// Token: 0x060008CD RID: 2253 RVA: 0x00019AEC File Offset: 0x00017CEC
		public void Restore(Adventure adventure)
		{
			this.Adventure = adventure;
			this.Adventure.SetStation(this);
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x00019B01 File Offset: 0x00017D01
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
