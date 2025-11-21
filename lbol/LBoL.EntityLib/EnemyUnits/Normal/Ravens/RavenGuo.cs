using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Ravens
{
	// Token: 0x020001F3 RID: 499
	[UsedImplicitly]
	public sealed class RavenGuo : Raven
	{
		// Token: 0x060007E9 RID: 2025 RVA: 0x000119C1 File Offset: 0x0000FBC1
		protected override IEnumerable<BattleAction> News()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Sfx("RavenGuo", 0f);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<HatateNews>(base.Count2, false), AddCardsType.Normal);
			yield break;
		}
	}
}
