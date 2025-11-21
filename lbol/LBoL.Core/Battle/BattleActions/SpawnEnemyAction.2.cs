using System;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A3 RID: 419
	[ActionViewerType(typeof(SpawnEnemyAction))]
	public sealed class SpawnEnemyAction<TEnemyUnit> : SpawnEnemyAction where TEnemyUnit : EnemyUnit
	{
		// Token: 0x06000F28 RID: 3880 RVA: 0x00028DD9 File Offset: 0x00026FD9
		public SpawnEnemyAction(EnemyUnit spawner, int rootIndex, float waitTime = 0f, float fadeInDelay = 0.3f, bool isServant = true)
			: base(spawner, typeof(TEnemyUnit), rootIndex, waitTime, fadeInDelay, isServant)
		{
		}
	}
}
