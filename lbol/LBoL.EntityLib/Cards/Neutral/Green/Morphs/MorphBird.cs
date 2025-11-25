using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Neutral.Green.Morphs
{
	[UsedImplicitly]
	public sealed class MorphBird : OptionCard
	{
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
