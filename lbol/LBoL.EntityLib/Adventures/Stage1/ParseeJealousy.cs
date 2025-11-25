using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Randoms;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage1
{
	[AdventureInfo(WeighterType = typeof(ParseeJealousy.ParseeJealousyWeighter))]
	public sealed class ParseeJealousy : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			Exhibit exhibit = base.Stage.RollExhibitInAdventure(new ExhibitWeightTable(new RarityWeightTable(0.5f, 0.33f, 0.17f, 0f), new AppearanceWeightTable(1f, 0f, 1f, 0f)), (ExhibitConfig config) => config.LosableType == ExhibitLosableType.Losable);
			storage.SetValue("$exhibitPassBy", exhibit.Id);
		}
		[RuntimeCommand("getExhibit", "")]
		[UsedImplicitly]
		public void GetExhibit()
		{
			Exhibit[] array = Enumerable.Where<Exhibit>(base.GameRun.Player.Exhibits, (Exhibit e) => e.LosableType == ExhibitLosableType.Losable).SampleManyOrAll(2, base.GameRun.AdventureRng);
			if (array[0] != null)
			{
				base.Storage.SetValue("$canExchangeExhibit", true);
				base.Storage.SetValue("$exhibit", array[0].Id);
			}
			if (array[1] != null)
			{
				base.Storage.SetValue("$canExchangeExhibit2", true);
				base.Storage.SetValue("$exhibit2", array[1].Id);
			}
		}
		private class ParseeJealousyWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((Enumerable.Count<Exhibit>(gameRun.Player.Exhibits, (Exhibit e) => e.LosableType == ExhibitLosableType.Losable) > 0) ? 1 : 0);
			}
		}
	}
}
