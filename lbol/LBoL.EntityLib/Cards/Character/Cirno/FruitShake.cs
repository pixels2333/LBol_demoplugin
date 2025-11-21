using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004BC RID: 1212
	[UsedImplicitly]
	public sealed class FruitShake : Card
	{
		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06001010 RID: 4112 RVA: 0x0001C78C File Offset: 0x0001A98C
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06001011 RID: 4113 RVA: 0x0001C794 File Offset: 0x0001A994
		public override bool IsForceCost
		{
			get
			{
				BattleController battle = base.Battle;
				return battle != null && battle.PlayerSummonAFriendThisTurn;
			}
		}

		// Token: 0x06001012 RID: 4114 RVA: 0x0001C7B4 File Offset: 0x0001A9B4
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarted), (GameEventPriority)0);
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageDealt), (GameEventPriority)0);
			this._coldingEnemies = new List<Unit>();
		}

		// Token: 0x06001013 RID: 4115 RVA: 0x0001C812 File Offset: 0x0001AA12
		private void OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Zone == CardZone.Hand)
			{
				this.NotifyChanged();
			}
		}

		// Token: 0x06001014 RID: 4116 RVA: 0x0001C824 File Offset: 0x0001AA24
		private void OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (args.ActionSource == this && !args.DamageInfo.IsGrazed)
			{
				Unit target = args.Target;
				this._coldingEnemies.Add(target);
			}
		}

		// Token: 0x06001015 RID: 4117 RVA: 0x0001C85D File Offset: 0x0001AA5D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd || this._coldingEnemies.Count == 0)
			{
				yield break;
			}
			foreach (Unit unit in this._coldingEnemies)
			{
				if (unit.IsAlive)
				{
					yield return base.DebuffAction<Cold>(unit, 0, 0, 0, 0, true, 0.03f);
				}
			}
			List<Unit>.Enumerator enumerator = default(List<Unit>.Enumerator);
			this._coldingEnemies.Clear();
			yield break;
			yield break;
		}

		// Token: 0x0400010C RID: 268
		private List<Unit> _coldingEnemies;
	}
}
