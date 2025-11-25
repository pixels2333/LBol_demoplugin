using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class StarlightTaifengSummoner : Card
	{
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return new ManaGroup
			{
				Black = pooledMana.Black,
				Red = pooledMana.Red,
				Philosophy = pooledMana.Philosophy
			};
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			StarlightTaifeng starlightTaifeng = Library.CreateCard<StarlightTaifeng>();
			starlightTaifeng.DeltaDamage = base.SynergyAmount(consumingMana, ManaColor.Red, 1) * base.Value1;
			starlightTaifeng.DeltaValue1 = base.SynergyAmount(consumingMana, ManaColor.Black, 2);
			yield return new AddCardsToDrawZoneAction(new StarlightTaifeng[] { starlightTaifeng }, this.IsUpgraded ? DrawZoneTarget.Top : DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
