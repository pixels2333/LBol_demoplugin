using System;
using System.Collections;
using LBoL.Core.Units;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	public interface IKokoroView : IEnemyUnitView, IUnitView
	{
		void SetEffect(SkirtColor skirtColor);
		IEnumerator SwitchToFace(SkirtColor skirtColor);
	}
}
