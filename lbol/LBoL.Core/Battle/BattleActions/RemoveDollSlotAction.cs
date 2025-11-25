using System;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class RemoveDollSlotAction : SimpleAction
	{
		public RemoveDollSlotAction(int count)
		{
			this._count = count;
		}
		protected override void ResolvePhase()
		{
			PlayerUnit player = base.Battle.Player;
			if (player.DollSlotCount < this._count)
			{
				base.Battle.NotifyMessage(BattleMessage.DollSlotNotEnough);
				player.SetDollSlot(0);
				return;
			}
			player.SetDollSlot(player.DollSlotCount - this._count);
		}
		public override string ExportDebugDetails()
		{
			return this._count.ToString();
		}
		private readonly int _count;
	}
}
