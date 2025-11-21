using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x0200024E RID: 590
	[UsedImplicitly]
	public sealed class Youmu : EnemyUnit
	{
		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06000978 RID: 2424 RVA: 0x00014673 File Offset: 0x00012873
		// (set) Token: 0x06000979 RID: 2425 RVA: 0x0001467B File Offset: 0x0001287B
		private Youmu.MoveType Next { get; set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x0600097A RID: 2426 RVA: 0x00014684 File Offset: 0x00012884
		private string Move0
		{
			get
			{
				return base.GetSpellCardName(default(int?), 0);
			}
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x0600097B RID: 2427 RVA: 0x000146A1 File Offset: 0x000128A1
		private string Move1
		{
			get
			{
				return base.GetSpellCardName(new int?(1), 2);
			}
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x0600097C RID: 2428 RVA: 0x000146B0 File Offset: 0x000128B0
		private string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(3), 4);
			}
		}

		// Token: 0x0600097D RID: 2429 RVA: 0x000146BF File Offset: 0x000128BF
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Youmu.MoveType.MultiAttack;
			base.CountDown = 4;
			this.AttackCount = 1;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x0600097E RID: 2430 RVA: 0x000146F3 File Offset: 0x000128F3
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			if (base.GameRun.ExtraFlags.Contains("YoumuMooncake"))
			{
				SadinYuebing sadinYuebing = Library.CreateExhibit<SadinYuebing>();
				string text = "Chat.YoumuMooncake".LocalizeFormat(new object[] { sadinYuebing.GetName() });
				yield return PerformAction.Chat(this, text, 3f, 1f, 0f, true);
			}
			else if (base.GameRun.ExtraFlags.Contains("YoumuTuanzi"))
			{
				yield return PerformAction.Chat(this, "Chat.YoumuTuanzi".Localize(true), 3f, 1f, 0f, true);
			}
			else
			{
				yield return PerformAction.Chat(this, "Chat.Youmu1".Localize(true), 3f, 1f, 0f, true);
			}
			yield return new ApplyStatusEffectAction<LouguanJianSe>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600097F RID: 2431 RVA: 0x00014703 File Offset: 0x00012903
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Youmu.MoveType.MultiAttack:
				yield return base.AttackMove(this.Move0, base.Gun1, base.Damage1, base.Count1, false, base.Gun1, true);
				yield return base.DefendMove(this, null, 0, 0, base.Defend, false, null);
				break;
			case Youmu.MoveType.AttackAndAddCard:
				yield return base.AttackMove(this.Move1, base.Gun2, base.Damage2, 1, false, "Instant", true);
				yield return base.AddCardMove(null, Library.CreateCards<Wushuai>(base.Count2, false), EnemyUnit.AddCardZone.Draw, null, false);
				break;
			case Youmu.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCard, new int?(base.Damage3), new int?(3), true), this.AttackActions(this.SpellCard, base.Gun3, base.Damage3, 3, true, "Instant"));
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			int countDown = base.CountDown;
			if (countDown == 1 || countDown == 2)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000980 RID: 2432 RVA: 0x00014713 File Offset: 0x00012913
		// (set) Token: 0x06000981 RID: 2433 RVA: 0x0001471B File Offset: 0x0001291B
		private int AttackCount { get; set; }

		// Token: 0x06000982 RID: 2434 RVA: 0x00014724 File Offset: 0x00012924
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Youmu.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
				return;
			}
			if (this.AttackCount <= 0)
			{
				this.Next = Youmu.MoveType.AttackAndAddCard;
				this.AttackCount = base.EnemyMoveRng.NextInt(1, 2);
				return;
			}
			this.Next = Youmu.MoveType.MultiAttack;
			num = this.AttackCount - 1;
			this.AttackCount = num;
		}

		// Token: 0x0200078A RID: 1930
		private enum MoveType
		{
			// Token: 0x04000BF6 RID: 3062
			MultiAttack,
			// Token: 0x04000BF7 RID: 3063
			AttackAndAddCard,
			// Token: 0x04000BF8 RID: 3064
			Spell
		}
	}
}
