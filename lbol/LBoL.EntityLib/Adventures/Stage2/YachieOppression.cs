using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Randoms;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage2
{
	// Token: 0x0200050B RID: 1291
	public sealed class YachieOppression : Adventure
	{
		// Token: 0x060010FB RID: 4347 RVA: 0x0001E990 File Offset: 0x0001CB90
		protected override void InitVariables(IVariableStorage storage)
		{
			string text = new UniqueRandomPool<string>(true) { { "YachieBattle", 1f } }.Sample(base.GameRun.AdventureRng);
			storage.SetValue("$randomElite", text);
			Exhibit exhibit = base.Stage.RollExhibitInAdventure(new ExhibitWeightTable(RarityWeightTable.OnlyRare, new AppearanceWeightTable(1f, 0f, 1f, 0f)), null);
			storage.SetValue("$enemyExhibit", exhibit.Id);
		}
	}
}
