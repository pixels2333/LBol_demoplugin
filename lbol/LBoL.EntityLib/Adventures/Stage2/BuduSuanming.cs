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
	[AdventureInfo(WeighterType = typeof(BuduSuanming.BuduSuanmingWeighter))]
	public sealed class BuduSuanming : Adventure
	{
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
		public BuduSuanming()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(Shendeng));
			list.Add(typeof(SihunYu));
			list.Add(typeof(MadokaBow));
			this._pool = list;
			base..ctor();
		}
		private const int CostMoney = 10;
		private readonly List<Type> _pool;
		private class BuduSuanmingWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Money >= 10) ? 1 : 0);
			}
		}
	}
}
