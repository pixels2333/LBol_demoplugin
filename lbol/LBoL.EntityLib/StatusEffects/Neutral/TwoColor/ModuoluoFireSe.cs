using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	public sealed class ModuoluoFireSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}
		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			ManaGroup value = args.Value;
			if (value.Philosophy > 0)
			{
				base.NotifyActivating();
				int num = base.Level * value.Philosophy;
				string text = "秽火";
				if (num > 10)
				{
					text = "秽火B";
				}
				if (num > 20)
				{
					text = "秽火C";
				}
				yield return new DamageAction(base.Battle.Player, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)num, false), text, GunType.Single);
			}
			yield break;
		}
	}
}
