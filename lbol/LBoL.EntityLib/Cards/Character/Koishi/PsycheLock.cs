using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn.Partners;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000491 RID: 1169
	[UsedImplicitly]
	public sealed class PsycheLock : Card
	{
		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000F9A RID: 3994 RVA: 0x0001BD31 File Offset: 0x00019F31
		[UsedImplicitly]
		public int Count
		{
			get
			{
				return 3 - this._moods.Count;
			}
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000F9B RID: 3995 RVA: 0x0001BD40 File Offset: 0x00019F40
		public override bool CanUse
		{
			get
			{
				return this.Count == 0;
			}
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000F9C RID: 3996 RVA: 0x0001BD4B File Offset: 0x00019F4B
		public override bool Triggered
		{
			get
			{
				return this.Count == 0;
			}
		}

		// Token: 0x06000F9D RID: 3997 RVA: 0x0001BD56 File Offset: 0x00019F56
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
			yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				yield return base.BuffAction<UpgradeAllHandSe>(0, 0, 0, 0, 0.2f);
			}
			yield return new RequestEndPlayerTurnAction();
			yield break;
		}

		// Token: 0x06000F9E RID: 3998 RVA: 0x0001BD66 File Offset: 0x00019F66
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.Battle.Player.StatusEffectAdded, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnPlayerSeAdded), (GameEventPriority)0);
			if (base.Zone == CardZone.Hand)
			{
				this.EnterHand();
			}
		}

		// Token: 0x06000F9F RID: 3999 RVA: 0x0001BD9C File Offset: 0x00019F9C
		private void OnPlayerSeAdded(StatusEffectApplyEventArgs args)
		{
			if (base.Zone == CardZone.Hand)
			{
				Mood mood = args.Effect as Mood;
				if (mood != null && !this._moods.Contains(mood.GetType()))
				{
					this._moods.Add(mood.GetType());
					this.NotifyChanged();
				}
			}
		}

		// Token: 0x06000FA0 RID: 4000 RVA: 0x0001BDEB File Offset: 0x00019FEB
		public override IEnumerable<BattleAction> OnDraw()
		{
			this.EnterHand();
			return null;
		}

		// Token: 0x06000FA1 RID: 4001 RVA: 0x0001BDF4 File Offset: 0x00019FF4
		public override IEnumerable<BattleAction> OnMove(CardZone srcZone, CardZone dstZone)
		{
			if (dstZone == CardZone.Hand)
			{
				this.EnterHand();
			}
			return null;
		}

		// Token: 0x06000FA2 RID: 4002 RVA: 0x0001BE04 File Offset: 0x0001A004
		private void EnterHand()
		{
			StatusEffect statusEffect = Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood && !this._moods.Contains(se.GetType()));
			if (statusEffect != null)
			{
				this._moods.Add(statusEffect.GetType());
			}
		}

		// Token: 0x06000FA3 RID: 4003 RVA: 0x0001BE47 File Offset: 0x0001A047
		public override void OnLeaveHand()
		{
			base.OnLeaveHand();
			this._moods.Clear();
		}

		// Token: 0x0400010A RID: 266
		private readonly List<Type> _moods = new List<Type>();
	}
}
