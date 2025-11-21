using System;
using System.Collections.Generic;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A6 RID: 422
	public sealed class StartEnemyTurnAction : SimpleAction
	{
		// Token: 0x17000527 RID: 1319
		// (get) Token: 0x06000F30 RID: 3888 RVA: 0x00028F0C File Offset: 0x0002710C
		public EnemyUnit Unit { get; }

		// Token: 0x06000F31 RID: 3889 RVA: 0x00028F14 File Offset: 0x00027114
		internal StartEnemyTurnAction(EnemyUnit enemy)
		{
			this.Unit = enemy;
			this._args = new UnitEventArgs
			{
				Unit = enemy,
				CanCancel = false
			};
		}

		// Token: 0x06000F32 RID: 3890 RVA: 0x00028F3C File Offset: 0x0002713C
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Viewer", delegate
			{
			}, true);
			yield return base.CreatePhase("LoseBlockGraze", delegate
			{
				TurnStartDontLoseBlock statusEffect = this.Unit.GetStatusEffect<TurnStartDontLoseBlock>();
				if (statusEffect != null)
				{
					TurnStartDontLoseBlock turnStartDontLoseBlock = statusEffect;
					int num = turnStartDontLoseBlock.Level - 1;
					turnStartDontLoseBlock.Level = num;
					if (statusEffect.Level == 0)
					{
						base.React(new RemoveStatusEffectAction(statusEffect, true, 0.1f), null, default(ActionCause?));
					}
				}
				else
				{
					base.React(new LoseBlockShieldAction(this.Unit, this.Unit.Block, 0, false), null, new ActionCause?(ActionCause.TurnStart));
				}
				if (!this.Unit.HasStatusEffect<WindGirl>())
				{
					Graze statusEffect2 = this.Unit.GetStatusEffect<Graze>();
					if (statusEffect2 == null)
					{
						return;
					}
					statusEffect2.LoseGraze();
				}
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnStarting", this._args, this.Unit.TurnStarting);
			yield return base.CreatePhase("DecreaseDuration", delegate
			{
				base.Battle.TurnStartDecreaseDuration(this.Unit);
			}, false);
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.StartEnemyTurn(this.Unit);
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnStarted", this._args, this.Unit.TurnStarted);
			yield return base.CreatePhase("IsInTurn", delegate
			{
				this.Unit.IsInTurn = true;
			}, false);
			yield break;
		}

		// Token: 0x040006A0 RID: 1696
		private readonly UnitEventArgs _args;
	}
}
