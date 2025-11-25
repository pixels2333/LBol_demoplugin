using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class LoseMoneyAction : SimpleAction
	{
		public int Money { get; }
		public LoseMoneyAction(int money)
		{
			this.Money = money;
		}
		protected override void ResolvePhase()
		{
			base.Battle.GameRun.LoseMoney(this.Money);
		}
	}
}
