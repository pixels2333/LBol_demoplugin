using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class LunaticPassion : Card
	{
		[UsedImplicitly]
		public int Percentage
		{
			get
			{
				return 150;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<LunaticPassionSe>(0, 0, 0, 0, 0.2f);
			yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
