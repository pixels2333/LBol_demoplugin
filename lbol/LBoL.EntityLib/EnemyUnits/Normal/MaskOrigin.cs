using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public abstract class MaskOrigin : EnemyUnit
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Servant>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(PerformAction.Sfx("GhostSpawn", 0f));
		}
		private IEnumerable<BattleAction> Debuff()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			if (base.EnemyBattleRng.NextInt(0, 1) == 0)
			{
				yield return new ApplyStatusEffectAction<TempFirepowerNegative>(base.Battle.Player, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return new ApplyStatusEffectAction<TempSpiritNegative>(base.Battle.Player, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			if ((base.TurnCounter + base.RootIndex) % 3 == 0)
			{
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.Debuff());
			}
			else
			{
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, base.Damage2), 1, false, "Instant", false);
			}
			yield break;
		}
	}
}
