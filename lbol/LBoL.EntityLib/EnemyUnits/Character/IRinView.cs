using System;
using System.Collections;
using LBoL.Core.Units;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000244 RID: 580
	public interface IRinView : IEnemyUnitView, IUnitView
	{
		// Token: 0x06000911 RID: 2321
		void SetOrb(string effectName, int orbitIndex);

		// Token: 0x06000912 RID: 2322
		IEnumerator MoveOrbToEnemy(EnemyUnit enemy);

		// Token: 0x06000913 RID: 2323
		IEnumerator RecycleOrb();
	}
}
