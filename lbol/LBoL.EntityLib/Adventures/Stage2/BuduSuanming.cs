using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Exhibits.Seija;
using LBoL.EntityLib.Stages.NormalStages;
using UnityEngine;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage2
{
	// Token: 0x02000508 RID: 1288
	[AdventureInfo(WeighterType = typeof(BuduSuanming.BuduSuanmingWeighter))]
	public sealed class BuduSuanming : Adventure
	{
		// Token: 0x060010F2 RID: 4338 RVA: 0x0001E5BC File Offset: 0x0001C7BC
		protected override void InitVariables(IVariableStorage storage)
		{
			Stage stage3 = Enumerable.FirstOrDefault<Stage>(base.GameRun.Stages, (Stage stage) => stage.Level == 3);
			if (stage3 == null)
			{
				Debug.LogWarning("布都算命无法找到第三幕");
				stage3 = base.GameRun.CurrentStage;
			}
			string id = stage3.Boss.Id;
			string nameWithColor = LBoL.Core.Library.CreateEnemyUnit(id).NameWithColor;
			storage.SetValue("$money", 10f);
			storage.SetValue("$bossId", id);
			storage.SetValue("$bossName", nameWithColor);
			if (stage3.FirstAdventure != null)
			{
				Adventure adventure = LBoL.Core.Library.CreateAdventure(stage3.FirstAdventure);
				string hostName = adventure.HostName;
				string title = adventure.Title;
				AdventureConfig adventureConfig = AdventureConfig.FromId(adventure.Id);
				if (adventureConfig == null)
				{
					throw new InvalidOperationException("Lack of adventure config for " + adventure.Id);
				}
				string hostId = adventureConfig.HostId;
				storage.SetValue("$firstFlag", true);
				storage.SetValue("$hostId", hostId);
				storage.SetValue("$hostName", hostName);
				storage.SetValue("$firstName", title);
			}
			FinalStage finalStage = null;
			foreach (Stage stage2 in base.GameRun.Stages)
			{
				FinalStage finalStage2 = stage2 as FinalStage;
				if (finalStage2 != null)
				{
					finalStage = finalStage2;
					break;
				}
			}
			if (finalStage != null)
			{
				RandomGen randomGen = new RandomGen(base.GameRun.FinalBossSeed);
				Exhibit exhibit = LBoL.Core.Library.CreateExhibit(this._pool.Sample(randomGen));
				storage.SetValue("$seijaFlag", true);
				storage.SetValue("$seijaItem", exhibit.Id);
				storage.SetValue("$seijaItemName", exhibit.Name);
			}
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x0001E78C File Offset: 0x0001C98C
		public BuduSuanming()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(Shendeng));
			list.Add(typeof(SihunYu));
			list.Add(typeof(MadokaBow));
			this._pool = list;
			base..ctor();
		}

		// Token: 0x04000129 RID: 297
		private const int CostMoney = 10;

		// Token: 0x0400012A RID: 298
		private readonly List<Type> _pool;

		// Token: 0x02000A64 RID: 2660
		private class BuduSuanmingWeighter : IAdventureWeighter
		{
			// Token: 0x06003742 RID: 14146 RVA: 0x000864BD File Offset: 0x000846BD
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Money >= 10) ? 1 : 0);
			}
		}
	}
}
