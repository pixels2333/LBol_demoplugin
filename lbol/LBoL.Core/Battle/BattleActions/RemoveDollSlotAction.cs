using System;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200019C RID: 412
	public sealed class RemoveDollSlotAction : SimpleAction
	{
		// Token: 0x06000F0F RID: 3855 RVA: 0x00028A86 File Offset: 0x00026C86
		public RemoveDollSlotAction(int count)
		{
			this._count = count;
		}

		// Token: 0x06000F10 RID: 3856 RVA: 0x00028A98 File Offset: 0x00026C98
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

		// Token: 0x06000F11 RID: 3857 RVA: 0x00028AE6 File Offset: 0x00026CE6
		public override string ExportDebugDetails()
		{
			return this._count.ToString();
		}

		// Token: 0x04000697 RID: 1687
		private readonly int _count;
	}
}
