using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class SanaeSummonGodSe : StatusEffect
	{
		protected override string GetBaseDescription()
		{
			if (base.Count != 1)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			int num = base.Count - 1;
			base.Count = num;
			if (base.Count <= 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
				yield return new ApplyStatusEffectAction<Spirit>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
