using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class PassionVulnerable : Card
	{
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodPassion>();
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.TriggeredAnyhow)
			{
				foreach (BattleAction battleAction in base.DebuffAction<Vulnerable>(selector.GetUnits(base.Battle), 0, base.Value1, 0, 0, true, 0.2f))
				{
					yield return battleAction;
				}
				IEnumerator<BattleAction> enumerator = null;
				if (base.Value2 > 0)
				{
					foreach (BattleAction battleAction2 in base.DebuffAction<LockedOn>(selector.GetUnits(base.Battle), base.Value2, 0, 0, 0, true, 0.2f))
					{
						yield return battleAction2;
					}
					enumerator = null;
				}
			}
			else
			{
				yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
			}
			yield break;
			yield break;
		}
	}
}
