using System;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000160 RID: 352
	public sealed class AddDollSlotAction : SimpleAction
	{
		// Token: 0x06000DCF RID: 3535 RVA: 0x00025EDC File Offset: 0x000240DC
		public AddDollSlotAction(int count)
		{
			this._count = count;
		}

		// Token: 0x06000DD0 RID: 3536 RVA: 0x00025EEC File Offset: 0x000240EC
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

		// Token: 0x06000DD1 RID: 3537 RVA: 0x00025F3C File Offset: 0x0002413C
		public override string ExportDebugDetails()
		{
			return this._count.ToString();
		}

		// Token: 0x04000656 RID: 1622
		private readonly int _count;
	}
}
