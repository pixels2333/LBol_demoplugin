using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class GainMoneyAction : SimpleAction
	{
		public int Money { get; }
		public SpecialSourceType SpecialSource { get; }
		public GainMoneyAction(int money, SpecialSourceType type = SpecialSourceType.None)
		{
			this.Money = money;
			this.SpecialSource = type;
		}
		protected override void ResolvePhase()
		{
			base.Battle.GameRun.GainMoney(this.Money, false, null);
		}
	}
}
