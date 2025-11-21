using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Red
{
	// Token: 0x02000054 RID: 84
	public sealed class MeihongPowerSe : StatusEffect
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000119 RID: 281 RVA: 0x000040EF File Offset: 0x000022EF
		[UsedImplicitly]
		public int Heal
		{
			get
			{
				return base.Level * 2;
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x000040F9 File Offset: 0x000022F9
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarting));
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(this.OnBattleEnding));
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00004135 File Offset: 0x00002335
		private IEnumerable<BattleAction> OnOwnerTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00004145 File Offset: 0x00002345
		private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
		{
			if (base.Battle.Player.IsAlive && base.Battle.Player.Hp <= (base.Battle.Player.MaxHp + 1) / 2)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Battle.Player, base.Battle.Player, this.Heal, HealType.Normal, 0.2f);
			}
			yield break;
		}
	}
}
