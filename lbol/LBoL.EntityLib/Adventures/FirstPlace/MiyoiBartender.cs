using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Stages.NormalStages;
using Yarn;

namespace LBoL.EntityLib.Adventures.FirstPlace
{
	// Token: 0x0200051F RID: 1311
	public sealed class MiyoiBartender : Adventure
	{
		// Token: 0x06001132 RID: 4402 RVA: 0x0001FC68 File Offset: 0x0001DE68
		protected override void InitVariables(IVariableStorage storage)
		{
			UniqueRandomPool<string> uniqueRandomPool = new UniqueRandomPool<string>(true)
			{
				{ "37", 1f },
				{ "38", 1f },
				{ "39", 1f }
			};
			if (base.Stage is WindGodLake)
			{
				uniqueRandomPool = base.Stage.EnemyPoolAct3;
			}
			string text = uniqueRandomPool.Sample(base.GameRun.StationRng);
			if (text == null)
			{
				throw new ArgumentNullException();
			}
			string text2 = text;
			storage.SetValue("$randomOpponent", text2);
			Exhibit exhibit = base.Stage.RollExhibitInAdventure(new ExhibitWeightTable(RarityWeightTable.OnlyRare, new AppearanceWeightTable(1f, 0f, 1f, 0f)), null);
			storage.SetValue("$isSentinel", exhibit.Config.IsSentinel);
			storage.SetValue("$randomExhibit", exhibit.Id);
		}
	}
}
