using System;
namespace LBoL.Core.Units
{
	public class EnemyUnit<TView> : EnemyUnit where TView : IEnemyUnitView
	{
		public override void SetView(IUnitView view)
		{
			base.SetView(view);
			if (view is TView)
			{
				TView tview = (TView)((object)view);
				this.View = tview;
			}
		}
		public new TView View { get; private set; }
	}
}
