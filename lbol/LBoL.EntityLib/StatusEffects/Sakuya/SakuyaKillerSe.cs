using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;
namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	[UsedImplicitly]
	public sealed class SakuyaKillerSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
			base.ReactOwnerEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnding(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ActiveAction();
			yield break;
		}
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return this.ActiveAction();
			yield break;
		}
		private BattleAction ActiveAction()
		{
			base.NotifyActivating();
			return new AddCardsToDrawZoneAction(Library.CreateCards<Knife>(base.Level, false), DrawZoneTarget.Random, AddCardsType.Normal);
		}
	}
}
