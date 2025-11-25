using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.Green
{
	public sealed class HuiyeManaSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.Count = 0;
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerStarted));
		}
		private IEnumerable<BattleAction> OnOwnerStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			base.Count += base.Level;
			ManaGroup manaGroup = ManaGroup.Empty;
			for (int i = 0; i < base.Count; i++)
			{
				manaGroup += ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
			}
			yield return new GainManaAction(manaGroup);
			yield break;
		}
	}
}
