using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class IceLance : Card
	{
		protected override int AdditionalValue1
		{
			get
			{
				if (base.Battle == null || !base.Battle.Player.HasStatusEffect<ColdHeartedSe>())
				{
					return 0;
				}
				return 1;
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Battle.Player.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing), GameEventPriority.ConfigDefault);
		}
		private void OnPlayerDamageDealing(DamageDealingEventArgs args)
		{
			if (args.ActionSource == this && args.Targets != null)
			{
				if (Enumerable.Any<Unit>(args.Targets, (Unit target) => target.HasStatusEffect<Cold>()))
				{
					args.DamageInfo = args.DamageInfo.MultiplyBy(base.Value1);
					args.AddModifier(this);
				}
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			string text = base.GunName;
			if (base.Battle.Player.HasStatusEffect<ColdHeartedSe>())
			{
				text = "血脉" + base.GunName;
			}
			yield return base.AttackAction(selector, text);
			yield break;
		}
	}
}
