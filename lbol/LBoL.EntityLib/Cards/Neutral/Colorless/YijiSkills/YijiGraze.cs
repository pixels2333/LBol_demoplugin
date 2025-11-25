using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Neutral.Colorless.YijiSkills
{
	[UsedImplicitly]
	public sealed class YijiGraze : OptionCard
	{
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
