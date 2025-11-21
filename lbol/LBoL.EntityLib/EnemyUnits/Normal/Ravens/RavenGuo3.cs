using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Ravens
{
	// Token: 0x020001F4 RID: 500
	[UsedImplicitly]
	public sealed class RavenGuo3 : Raven
	{
		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060007EB RID: 2027 RVA: 0x000119D9 File Offset: 0x0000FBD9
		protected override bool HasGraze
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060007EC RID: 2028 RVA: 0x000119DC File Offset: 0x0000FBDC
		protected override IEnumerable<BattleAction> News()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<HatateNews>(base.Count2, false), AddCardsType.Normal);
			yield break;
		}
	}
}
