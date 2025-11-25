using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class LunaDial : Card
	{
		public override bool OnDrawVisual
		{
			get
			{
				return false;
			}
		}
		public override bool OnMoveVisual
		{
			get
			{
				return false;
			}
		}
		public override IEnumerable<BattleAction> OnDraw()
		{
			return this.EnterHandReactor();
		}
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone != CardZone.Hand)
			{
				return null;
			}
			return this.EnterHandReactor();
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Zone == CardZone.Hand)
			{
				base.React(this.EnterHandReactor());
			}
		}
		private IEnumerable<BattleAction> EnterHandReactor()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone != CardZone.Hand)
			{
				Debug.LogWarning(this.Name + " is not in hand.");
				yield break;
			}
			base.DecreaseBaseCost(base.Mana);
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}
	}
}
