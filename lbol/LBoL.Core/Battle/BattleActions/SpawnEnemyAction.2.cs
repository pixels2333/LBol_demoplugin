using System;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	[ActionViewerType(typeof(SpawnEnemyAction))]
	public sealed class SpawnEnemyAction<TEnemyUnit> : SpawnEnemyAction where TEnemyUnit : EnemyUnit
	{
		public SpawnEnemyAction(EnemyUnit spawner, int rootIndex, float waitTime = 0f, float fadeInDelay = 0.3f, bool isServant = true)
			: base(spawner, typeof(TEnemyUnit), rootIndex, waitTime, fadeInDelay, isServant)
		{
		}
	}
}
