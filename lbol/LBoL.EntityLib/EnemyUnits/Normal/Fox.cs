using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;
using UnityEngine;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001D6 RID: 470
	[UsedImplicitly]
	public sealed class Fox : EnemyUnit
	{
		// Token: 0x170000AD RID: 173
		// (get) Token: 0x06000720 RID: 1824 RVA: 0x000103D1 File Offset: 0x0000E5D1
		// (set) Token: 0x06000721 RID: 1825 RVA: 0x000103D9 File Offset: 0x0000E5D9
		private Fox.MoveType Next { get; set; }

		// Token: 0x06000722 RID: 1826 RVA: 0x000103E2 File Offset: 0x0000E5E2
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Fox.MoveType.Debuff;
			base.CountDown = 2;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			this._chatIndicator = Random.Range(1, 3);
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x0001041C File Offset: 0x0000E61C
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Chat(this, "Chat.Fox0".Localize(true), 3f, 0f, 0f, true);
			yield break;
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x0001042C File Offset: 0x0000E62C
		private IEnumerable<BattleAction> CharmActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1);
			Unit player = base.Battle.Player;
			int? num = new int?(3);
			yield return new ApplyStatusEffectAction<FoxCharm>(player, default(int?), default(int?), default(int?), num, 0f, true);
			yield return PerformAction.Chat(this, ("Chat.Fox" + this._chatIndicator.ToString()).Localize(true), 3f, 0f, 2f, true);
			this._chatIndicator = 3 - this._chatIndicator;
			yield break;
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x0001043C File Offset: 0x0000E63C
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(base.GetMove(1), base.Gun2, base.Damage2, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			Unit player = base.Battle.Player;
			int? num = new int?(base.Power);
			yield return new ApplyStatusEffectAction<Weak>(player, default(int?), num, default(int?), default(int?), 0f, false);
			yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Chat(this, ("Chat.Fox" + this._chatIndicator.ToString()).Localize(true), 3f, 0f, 1f, true);
			this._chatIndicator = 3 - this._chatIndicator;
			yield break;
			yield break;
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x0001044C File Offset: 0x0000E64C
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Fox.MoveType.Attack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", true);
				break;
			case Fox.MoveType.AttackLarge:
				yield return new SimpleEnemyMove(Intention.SpellCard(base.GetMove(1), new int?(base.Damage2), true), this.SpellActions());
				break;
			case Fox.MoveType.Debuff:
				yield return new SimpleEnemyMove(Intention.NegativeEffect("Charm").WithMoveName(base.GetMove(2)), this.CharmActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x0001045C File Offset: 0x0000E65C
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = (base.Battle.Player.HasStatusEffect<FoxCharm>() ? Fox.MoveType.AttackLarge : Fox.MoveType.Debuff);
				base.CountDown = ((base.EnemyMoveRng.NextFloat(0f, 1f) < 0.8f) ? 3 : 2);
				return;
			}
			this.Next = Fox.MoveType.Attack;
		}

		// Token: 0x04000068 RID: 104
		private int _chatIndicator;

		// Token: 0x020006A4 RID: 1700
		private enum MoveType
		{
			// Token: 0x04000810 RID: 2064
			Attack,
			// Token: 0x04000811 RID: 2065
			AttackLarge,
			// Token: 0x04000812 RID: 2066
			Debuff
		}
	}
}
