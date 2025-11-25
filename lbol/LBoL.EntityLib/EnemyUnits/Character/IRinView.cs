using System;
using System.Collections;
using LBoL.Core.Units;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	public interface IRinView : IEnemyUnitView, IUnitView
	{
		void SetOrb(string effectName, int orbitIndex);
		IEnumerator MoveOrbToEnemy(EnemyUnit enemy);
		IEnumerator RecycleOrb();
	}
}
