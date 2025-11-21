using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage2
{
	// Token: 0x0200050A RID: 1290
	public sealed class RingoEmp : Adventure
	{
		// Token: 0x060010F8 RID: 4344 RVA: 0x0001E8E4 File Offset: 0x0001CAE4
		protected override void InitVariables(IVariableStorage storage)
		{
			this.tools = base.Stage.GetShopToolCards(3);
			storage.SetValue("$tool1", this.tools[0].Id);
			storage.SetValue("$tool2", this.tools[1].Id);
			storage.SetValue("$tool3", this.tools[2].Id);
		}

		// Token: 0x060010F9 RID: 4345 RVA: 0x0001E94C File Offset: 0x0001CB4C
		[RuntimeCommand("dropTools", "")]
		[UsedImplicitly]
		public void DropTools()
		{
			foreach (Card card in this.tools)
			{
				base.Station.Rewards.Add(StationReward.CreateToolCard(card));
			}
		}

		// Token: 0x0400012B RID: 299
		private Card[] tools;
	}
}
