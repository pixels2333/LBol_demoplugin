using System;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Cards.Adventure;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage1
{
	public sealed class AssistKagerou : Adventure
	{
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
