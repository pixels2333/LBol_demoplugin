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
	public sealed class KoishiFire : Card
	{
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && (base.Battle.Player.HasStatusEffect<MoodPeace>() || base.Battle.Player.HasStatusEffect<MoodPassion>());
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.TriggeredAnyhow)
			{
				if (base.Battle.Player.HasStatusEffect<MoodPeace>())
				{
					yield return base.BuffAction<Spirit>(base.Value1, 0, 0, 0, 0.2f);
				}
				else
				{
					yield return base.BuffAction<Firepower>(base.Value1 + base.Value2, 0, 0, 0, 0.2f);
				}
			}
			else
			{
				yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
