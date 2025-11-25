using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
namespace LBoL.EntityLib.EnemyUnits.Normal
{
	[UsedImplicitly]
	public sealed class WaterGirl : EnemyUnit
	{
		private WaterGirl.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = WaterGirl.MoveType.Debuff;
			base.CountDown = 2;
		}
		private IEnumerable<BattleAction> DrownActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<Drowning>(base.Battle.Player, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Chat(this, "Chat.WaterGirl".Localize(true), 3f, 0f, 0f, true);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case WaterGirl.MoveType.Attack:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 2, false, "Instant", false);
				break;
			case WaterGirl.MoveType.AttackLarge:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, true, "Instant", false);
				break;
			case WaterGirl.MoveType.Debuff:
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.DrownActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = (base.Battle.Player.HasStatusEffect<Drowning>() ? WaterGirl.MoveType.AttackLarge : WaterGirl.MoveType.Debuff);
				base.CountDown = ((base.EnemyMoveRng.NextFloat(0f, 1f) < 0.8f) ? 3 : 2);
				return;
			}
			this.Next = WaterGirl.MoveType.Attack;
		}
		private enum MoveType
		{
			Attack,
			AttackLarge,
			Debuff
		}
	}
}
