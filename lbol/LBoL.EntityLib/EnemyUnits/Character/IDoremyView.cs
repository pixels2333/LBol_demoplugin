using System;
using LBoL.Core.Units;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000238 RID: 568
	public interface IDoremyView : IEnemyUnitView, IUnitView
	{
		// Token: 0x06000896 RID: 2198
		void SetSleep(bool sleep);

		// Token: 0x06000897 RID: 2199
		void SetEffect(bool show, int level);
	}
}
