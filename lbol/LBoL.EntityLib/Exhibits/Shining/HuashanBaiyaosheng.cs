using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class HuashanBaiyaosheng : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			switch (base.Battle.Player.TurnCounter)
			{
			case 1:
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
				break;
			case 2:
				base.NotifyActivating();
				yield return new HealAction(base.Battle.Player, base.Battle.Player, base.Value1, HealType.Normal, 0.2f);
				break;
			case 3:
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Battle.Player, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
				break;
			}
			yield break;
		}
	}
}
