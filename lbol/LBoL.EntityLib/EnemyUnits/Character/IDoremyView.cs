using System;
using LBoL.Core.Units;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	public interface IDoremyView : IEnemyUnitView, IUnitView
	{
		void SetSleep(bool sleep);
		void SetEffect(bool show, int level);
	}
}
