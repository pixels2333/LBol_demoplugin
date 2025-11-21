using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.White
{
	// Token: 0x0200003E RID: 62
	[UsedImplicitly]
	public sealed class YukariFlyObjectSe : StatusEffect
	{
		// Token: 0x060000BD RID: 189 RVA: 0x00003590 File Offset: 0x00001790
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060000BE RID: 190 RVA: 0x000035B4 File Offset: 0x000017B4
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			EnemyUnit enemyUnit = base.Battle.EnemyGroup.Alives.MaxBy((EnemyUnit unit) => unit.Hp);
			string text = ((base.Level > 20) ? "超高速B" : "超高速");
			yield return new DamageAction(base.Owner, enemyUnit, DamageInfo.Reaction((float)base.Level, false), text, GunType.Single);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
