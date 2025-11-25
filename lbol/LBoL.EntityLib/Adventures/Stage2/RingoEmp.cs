using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage2
{
	public sealed class RingoEmp : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			this.tools = base.Stage.GetShopToolCards(3);
			storage.SetValue("$tool1", this.tools[0].Id);
			storage.SetValue("$tool2", this.tools[1].Id);
			storage.SetValue("$tool3", this.tools[2].Id);
		}
		[RuntimeCommand("dropTools", "")]
		[UsedImplicitly]
		public void DropTools()
		{
			foreach (Card card in this.tools)
			{
				base.Station.Rewards.Add(StationReward.CreateToolCard(card));
			}
		}
		private Card[] tools;
	}
}
