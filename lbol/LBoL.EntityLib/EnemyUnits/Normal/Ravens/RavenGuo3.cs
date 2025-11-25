using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Ravens
{
	[UsedImplicitly]
	public sealed class RavenGuo3 : Raven
	{
		protected override bool HasGraze
		{
			get
			{
				return false;
			}
		}
		protected override IEnumerable<BattleAction> News()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<HatateNews>(base.Count2, false), AddCardsType.Normal);
			yield break;
		}
	}
}
