using System;

namespace LBoL.Core.Units
{
	// Token: 0x0200007F RID: 127
	public class EnemyUnit<TView> : EnemyUnit where TView : IEnemyUnitView
	{
		// Token: 0x06000619 RID: 1561 RVA: 0x00013508 File Offset: 0x00011708
		public override void SetView(IUnitView view)
		{
			base.SetView(view);
			if (view is TView)
			{
				TView tview = (TView)((object)view);
				this.View = tview;
			}
		}

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x0600061A RID: 1562 RVA: 0x00013532 File Offset: 0x00011732
		// (set) Token: 0x0600061B RID: 1563 RVA: 0x0001353A File Offset: 0x0001173A
		public new TView View { get; private set; }
	}
}
