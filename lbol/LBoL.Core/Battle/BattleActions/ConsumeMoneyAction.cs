using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class ConsumeMoneyAction : SimpleAction
	{
		public int Money { get; }
		public ConsumeMoneyAction(int money)
		{
			this.Money = money;
		}
		protected override void ResolvePhase()
		{
			base.Battle.GameRun.ConsumeMoney(this.Money);
		}
		public override string ExportDebugDetails()
		{
			return string.Format("Money = {0}", this.Money);
		}
	}
}
