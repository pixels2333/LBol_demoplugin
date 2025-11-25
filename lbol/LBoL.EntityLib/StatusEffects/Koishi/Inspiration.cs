using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Koishi
{
	[UsedImplicitly]
	public sealed class Inspiration : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			string text = "Inspiration" + base.Level.ToString();
			this.React(PerformAction.Effect(base.Owner, text, 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
		}
		public override bool Stack(StatusEffect other)
		{
			string text = "Inspiration" + (base.Level + 1).ToString();
			this.React(PerformAction.Effect(base.Owner, text, 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
			return base.Stack(other);
		}
		public override IEnumerable<BattleAction> StackAction(Unit targetOwner, int targetLevel)
		{
			yield return new ApplyStatusEffectAction<MoodEpiphany>(targetOwner, new int?(targetLevel), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
