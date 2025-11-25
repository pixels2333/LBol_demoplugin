using System;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class AddDollSlotAction : SimpleAction
	{
		public AddDollSlotAction(int count)
		{
			this._count = count;
		}
		protected override void ResolvePhase()
		{
			PlayerUnit player = base.Battle.Player;
			if (player.DollSlotCount + this._count > 8)
			{
				base.Battle.NotifyMessage(BattleMessage.DollSlotFull);
				player.SetDollSlot(8);
				return;
			}
			player.SetDollSlot(player.DollSlotCount + this._count);
		}
		public override string ExportDebugDetails()
		{
			return this._count.ToString();
		}
		private readonly int _count;
	}
}
