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
	// Token: 0x02000371 RID: 881
	[UsedImplicitly]
	public sealed class Yueguang : Card
	{
		// Token: 0x06000C9F RID: 3231 RVA: 0x0001870F File Offset: 0x0001690F
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

		// Token: 0x06000CA0 RID: 3232 RVA: 0x0001871F File Offset: 0x0001691F
		private static ManaColor GetRandomColor(ManaGroup mana, RandomGen rng)
		{
			return Enumerable.ToList<ManaColor>(mana.EnumerateComponents()).Sample(rng);
		}

		// Token: 0x06000CA1 RID: 3233 RVA: 0x00018733 File Offset: 0x00016933
		private static ManaGroup GetRandomSingleMana(ManaGroup mana, RandomGen rng)
		{
			return ManaGroup.Single(Yueguang.GetRandomColor(mana, rng));
		}
	}
}
