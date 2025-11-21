using System;
using System.Collections;
using LBoL.Core.Units;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x0200023C RID: 572
	public interface IKokoroView : IEnemyUnitView, IUnitView
	{
		// Token: 0x060008C5 RID: 2245
		void SetEffect(SkirtColor skirtColor);

		// Token: 0x060008C6 RID: 2246
		IEnumerator SwitchToFace(SkirtColor skirtColor);
	}
}
