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
	// Token: 0x0200023F RID: 575
	public abstract class LightFairy : EnemyUnit
	{
		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x060008D7 RID: 2263 RVA: 0x000131F5 File Offset: 0x000113F5
		// (set) Token: 0x060008D8 RID: 2264 RVA: 0x000131FD File Offset: 0x000113FD
		private LightFairy.MoveType Next { get; set; }

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060008D9 RID: 2265 RVA: 0x00013206 File Offset: 0x00011406
		protected string LightMove
		{
			get
			{
				return base.GetSpellCardName(new int?(1), 2);
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x060008DA RID: 2266 RVA: 0x00013215 File Offset: 0x00011415
		protected string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(3), 4);
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x060008DB RID: 2267 RVA: 0x00013224 File Offset: 0x00011424
		protected virtual int AttackTimes
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x060008DC RID: 2268 RVA: 0x00013227 File Offset: 0x00011427
		public void Spell()
		{
			this.Next = LightFairy.MoveType.Spell;
		}

		// Token: 0x060008DD RID: 2269 RVA: 0x00013230 File Offset: 0x00011430
		public void Shoot()
		{
			this.Next = LightFairy.MoveType.Shoot;
		}

		// Token: 0x060008DE RID: 2270 RVA: 0x00013239 File Offset: 0x00011439
		public void Light()
		{
			this.Next = LightFairy.MoveType.Light;
		}

		// Token: 0x060008DF RID: 2271 RVA: 0x00013242 File Offset: 0x00011442
		protected virtual IEnumerable<BattleAction> LightActions()
		{
			yield return new EnemyMoveAction(this, this.LightMove, true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(new Card[] { Library.CreateCard<Riguang>() });
			yield break;
		}

		// Token: 0x060008E0 RID: 2272 RVA: 0x00013252 File Offset: 0x00011452
		protected virtual IEnumerable<BattleAction> SpellActions()
		{
			yield return PerformAction.Spell(this, "阳光直射");
			yield return new EnemyMoveAction(this, this.SpellCard, true);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Riguang>(base.Count1, false), AddCardsType.Normal);
			yield return PerformAction.Chat(this, "Chat.SunnySpell".Localize(true), 2.5f, 0.2f, 0f, true);
			yield break;
		}

		// Token: 0x060008E1 RID: 2273 RVA: 0x00013262 File Offset: 0x00011462
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

		// Token: 0x060008E2 RID: 2274 RVA: 0x00013274 File Offset: 0x00011474
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

		// Token: 0x02000743 RID: 1859
		protected enum MoveType
		{
			// Token: 0x04000AB8 RID: 2744
			Shoot,
			// Token: 0x04000AB9 RID: 2745
			Light,
			// Token: 0x04000ABA RID: 2746
			Spell
		}
	}
}
