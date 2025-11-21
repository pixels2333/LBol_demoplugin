using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Cards.Adventure;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage1
{
	// Token: 0x0200050C RID: 1292
	public sealed class AssistKagerou : Adventure
	{
		// Token: 0x060010FD RID: 4349 RVA: 0x0001EA1C File Offset: 0x0001CC1C
		protected override void InitVariables(IVariableStorage storage)
		{
			WolfFur wolfFur = LBoL.Core.Library.CreateCard<WolfFur>();
			storage.SetValue("$wolfFur", wolfFur.Id);
			Exhibit eliteEnemyExhibit = base.Stage.GetEliteEnemyExhibit();
			storage.SetValue("$isSentinel", eliteEnemyExhibit.Config.IsSentinel);
			base.Storage.SetValue("$exhibitReward", eliteEnemyExhibit.Config.Id);
		}
	}
}
