using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Cards.Neutral.Green.Morphs
{
	[UsedImplicitly]
	public sealed class MorphMan : OptionCard
	{
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return new GainPowerAction(base.Value1);
			yield break;
		}
	}
}
