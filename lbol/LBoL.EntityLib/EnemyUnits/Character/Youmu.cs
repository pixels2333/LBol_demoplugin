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
	[UsedImplicitly]
	public sealed class Youmu : EnemyUnit
	{
		private Youmu.MoveType Next { get; set; }
		private string Move0
		{
			get
			{
				return base.GetSpellCardName(default(int?), 0);
			}
		}
		private string Move1
		{
			get
			{
				return base.GetSpellCardName(new int?(1), 2);
			}
		}
		private string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(3), 4);
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Youmu.MoveType.MultiAttack;
			base.CountDown = 4;
			this.AttackCount = 1;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
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
		private int AttackCount { get; set; }
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
		private enum MoveType
		{
			MultiAttack,
			AttackAndAddCard,
			Spell
		}
	}
}
