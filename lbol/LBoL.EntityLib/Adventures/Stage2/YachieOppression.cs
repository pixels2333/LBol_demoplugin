using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Randoms;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage2
{
	public sealed class YachieOppression : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			string text = new UniqueRandomPool<string>(true) { { "YachieBattle", 1f } }.Sample(base.GameRun.AdventureRng);
			storage.SetValue("$randomElite", text);
			Exhibit exhibit = base.Stage.RollExhibitInAdventure(new ExhibitWeightTable(RarityWeightTable.OnlyRare, new AppearanceWeightTable(1f, 0f, 1f, 0f)), null);
			storage.SetValue("$enemyExhibit", exhibit.Id);
		}
	}
}
