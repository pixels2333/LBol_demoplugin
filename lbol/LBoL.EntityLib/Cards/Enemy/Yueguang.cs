using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Enemy
{
	[UsedImplicitly]
	public sealed class Yueguang : Card
	{
		public override IEnumerable<BattleAction> OnDraw()
		{
			ManaGroup manaGroup = ManaGroup.Empty;
			if (base.Value1 > 0 && base.Battle.BattleMana.Amount > 0)
			{
				if (base.Value1 >= base.Battle.BattleMana.Amount)
				{
					manaGroup = base.Battle.BattleMana;
				}
				else
				{
					foreach (ManaColor manaColor in base.Battle.BattleMana.EnumerateComponents().SampleManyOrAll(base.Value1, base.GameRun.BattleRng))
					{
						manaGroup += ManaGroup.Single(manaColor);
					}
				}
			}
			yield return new LoseManaAction(manaGroup);
			yield break;
		}
		private static ManaColor GetRandomColor(ManaGroup mana, RandomGen rng)
		{
			return Enumerable.ToList<ManaColor>(mana.EnumerateComponents()).Sample(rng);
		}
		private static ManaGroup GetRandomSingleMana(ManaGroup mana, RandomGen rng)
		{
			return ManaGroup.Single(Yueguang.GetRandomColor(mana, rng));
		}
	}
}
