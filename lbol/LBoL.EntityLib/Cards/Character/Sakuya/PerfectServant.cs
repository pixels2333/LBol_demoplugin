using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class PerfectServant : Card
	{
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return new ManaGroup
			{
				White = pooledMana.White,
				Blue = pooledMana.Blue,
				Philosophy = pooledMana.Philosophy
			};
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<PerfectServantUSe>(base.SynergyAmount(consumingMana, ManaColor.Blue, 1) * base.Value2, 0, 0, 0, 0.2f);
			yield return base.BuffAction<PerfectServantWSe>(base.SynergyAmount(consumingMana, ManaColor.White, 2) * base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
