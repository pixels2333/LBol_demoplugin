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
	[UsedImplicitly]
	public sealed class Fox : EnemyUnit
	{
		private Fox.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Fox.MoveType.Debuff;
			base.CountDown = 2;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			this._chatIndicator = Random.Range(1, 3);
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Chat(this, "Chat.Fox0".Localize(true), 3f, 0f, 0f, true);
			yield break;
		}
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
		private int _chatIndicator;
		private enum MoveType
		{
			Attack,
			AttackLarge,
			Debuff
		}
	}
}
