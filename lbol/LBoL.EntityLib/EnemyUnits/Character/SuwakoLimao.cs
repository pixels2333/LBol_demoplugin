using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	[UsedImplicitly]
	public sealed class SuwakoLimao : EnemyUnit
	{
		private SuwakoLimao.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = SuwakoLimao.MoveType.Start;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DieEventArgs>(base.Dying, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnDying));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return PerformAction.Effect(this, "Bianshen", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("Graze", 0f);
			yield return PerformAction.TransformModel(this, base.Id);
			yield return new ApplyStatusEffectAction<LimaoDisguiser>(this, default(int?), default(int?), default(int?), default(int?), 0.5f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(PerformAction.Effect(this, "Bianshen", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f));
			this.React(PerformAction.Sfx("Graze", 0f));
			this.React(PerformAction.TransformModel(this, base.Id));
			this.React(new ApplyStatusEffectAction<LimaoDisguiser>(this, default(int?), default(int?), default(int?), default(int?), 0.5f, true));
		}
		private IEnumerable<BattleAction> OnDying(DieEventArgs arg)
		{
			yield return PerformAction.Effect(this, "Bianshen", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("Graze", 0f);
			yield return PerformAction.TransformModel(this, "Limao");
			yield return PerformAction.DeathAnimation(this);
			yield break;
		}
		private IEnumerable<BattleAction> StartActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			yield return PerformAction.Chat(this, "Chat.Limao2".Localize(true), 3f, 0f, 1f, true);
			yield return PerformAction.Animation(this, "shoot2", 0.8f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<SuwakoHex>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		private IEnumerable<BattleAction> DebuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			Type typeFromHandle = typeof(Weak);
			Unit player = base.Battle.Player;
			int? num = new int?(base.Count1);
			yield return new ApplyStatusEffectAction(typeFromHandle, player, default(int?), num, default(int?), default(int?), 0f, false);
			yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
			Type typeFromHandle2 = typeof(Vulnerable);
			Unit player2 = base.Battle.Player;
			num = new int?(base.Count1);
			yield return new ApplyStatusEffectAction(typeFromHandle2, player2, default(int?), num, default(int?), default(int?), 0.3f, false);
			yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
			yield break;
		}
		private IEnumerable<BattleAction> HexActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			yield return PerformAction.Animation(this, "shoot2", 0.8f, null, 0f, -1);
			List<Card> cards = Enumerable.ToList<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(base.Battle.DrawZone, base.Battle.HandZone), base.Battle.DiscardZone).SampleManyOrAll(base.Count2, base.EnemyBattleRng));
			if (cards.Count > 0)
			{
				yield return PerformAction.UiSound("Frog");
				foreach (Card card in cards)
				{
					Frog frog = Library.CreateCard<Frog>();
					frog.OriginalCard = card;
					yield return new TransformCardAction(card, frog);
				}
				List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			}
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case SuwakoLimao.MoveType.Start:
				yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.StartActions());
				break;
			case SuwakoLimao.MoveType.ShootAndDefend:
				yield return base.AttackMove(base.GetMove(1), base.Gun1, base.Damage1, 1, false, "Instant", false);
				yield return base.DefendMove(this, null, 0, base.Defend, 0, false, null);
				break;
			case SuwakoLimao.MoveType.Debuff:
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.DebuffActions());
				break;
			case SuwakoLimao.MoveType.Hex:
				yield return new SimpleEnemyMove(Intention.Hex(), this.HexActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			SuwakoLimao.MoveType moveType;
			switch (this.Next)
			{
			case SuwakoLimao.MoveType.Start:
				moveType = SuwakoLimao.MoveType.ShootAndDefend;
				break;
			case SuwakoLimao.MoveType.ShootAndDefend:
				moveType = SuwakoLimao.MoveType.Debuff;
				break;
			case SuwakoLimao.MoveType.Debuff:
				moveType = SuwakoLimao.MoveType.Hex;
				break;
			case SuwakoLimao.MoveType.Hex:
				moveType = SuwakoLimao.MoveType.ShootAndDefend;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}
		private enum MoveType
		{
			Start,
			ShootAndDefend,
			Debuff,
			Hex
		}
	}
}
