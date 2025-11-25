using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	public abstract class LightFairy : EnemyUnit
	{
		private LightFairy.MoveType Next { get; set; }
		protected string LightMove
		{
			get
			{
				return base.GetSpellCardName(new int?(1), 2);
			}
		}
		protected string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(3), 4);
			}
		}
		protected virtual int AttackTimes
		{
			get
			{
				return 1;
			}
		}
		public void Spell()
		{
			this.Next = LightFairy.MoveType.Spell;
		}
		public void Shoot()
		{
			this.Next = LightFairy.MoveType.Shoot;
		}
		public void Light()
		{
			this.Next = LightFairy.MoveType.Light;
		}
		protected virtual IEnumerable<BattleAction> LightActions()
		{
			yield return new EnemyMoveAction(this, this.LightMove, true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<Riguang>() });
			yield break;
		}
		protected virtual IEnumerable<BattleAction> SpellActions()
		{
			yield return PerformAction.Spell(this, "阳光直射");
			yield return new EnemyMoveAction(this, this.SpellCard, true);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Riguang>(base.Count1, false), AddCardsType.Normal);
			yield return PerformAction.Chat(this, "Chat.SunnySpell".Localize(true), 2.5f, 0.2f, 0f, true);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			IEnemyMove enemyMove;
			switch (this.Next)
			{
			case LightFairy.MoveType.Shoot:
				enemyMove = base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, base.Damage2), this.AttackTimes, false, "Instant", false);
				break;
			case LightFairy.MoveType.Light:
				enemyMove = new SimpleEnemyMove(Intention.AddCard(), this.LightActions());
				break;
			case LightFairy.MoveType.Spell:
				enemyMove = new SimpleEnemyMove(Intention.SpellCard(this.SpellCard, default(int?), default(int?), false), this.SpellActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield return enemyMove;
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			LightFairy.MoveType moveType;
			switch (this.Next)
			{
			case LightFairy.MoveType.Shoot:
				moveType = LightFairy.MoveType.Light;
				break;
			case LightFairy.MoveType.Light:
				moveType = LightFairy.MoveType.Shoot;
				break;
			case LightFairy.MoveType.Spell:
				moveType = LightFairy.MoveType.Shoot;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			this.Next = moveType;
		}
		protected enum MoveType
		{
			Shoot,
			Light,
			Spell
		}
	}
}
