using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000344 RID: 836
	[UsedImplicitly]
	public sealed class YukariAttack : Card
	{
		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06000C27 RID: 3111 RVA: 0x00017D50 File Offset: 0x00015F50
		// (set) Token: 0x06000C28 RID: 3112 RVA: 0x00017D58 File Offset: 0x00015F58
		private bool TurnEndReturn { get; set; }

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000C29 RID: 3113 RVA: 0x00017D61 File Offset: 0x00015F61
		protected override int AdditionalDamage
		{
			get
			{
				return base.GrowCount * base.Value2;
			}
		}

		// Token: 0x06000C2A RID: 3114 RVA: 0x00017D70 File Offset: 0x00015F70
		protected override void SetGuns()
		{
			base.CardGuns = new Guns(base.GunName, base.Value1, true);
		}

		// Token: 0x06000C2B RID: 3115 RVA: 0x00017D8C File Offset: 0x00015F8C
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking), (GameEventPriority)0);
			base.HandleBattleEvent<HealEventArgs>(base.Battle.Player.HealingReceived, new GameEventHandler<HealEventArgs>(this.OnPlayerHealingReceived), (GameEventPriority)0);
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnded));
		}

		// Token: 0x06000C2C RID: 3116 RVA: 0x00017E04 File Offset: 0x00016004
		private void OnPlayerDamageTaking(DamageEventArgs args)
		{
			if (base.Battle.Player.IsInTurn && args.DamageInfo.Damage > 0f && !base.Battle.BattleShouldEnd)
			{
				this.TurnEndReturn = true;
				if (base.Zone == CardZone.Hand)
				{
					base.NotifyActivating();
				}
			}
		}

		// Token: 0x06000C2D RID: 3117 RVA: 0x00017E5B File Offset: 0x0001605B
		private void OnPlayerHealingReceived(HealEventArgs args)
		{
			if (base.Battle.Player.IsInTurn && !base.Battle.BattleShouldEnd)
			{
				this.TurnEndReturn = true;
				if (base.Zone == CardZone.Hand)
				{
					base.NotifyActivating();
				}
			}
		}

		// Token: 0x06000C2E RID: 3118 RVA: 0x00017E92 File Offset: 0x00016092
		private IEnumerable<BattleAction> OnPlayerTurnEnded(UnitEventArgs args)
		{
			if (this.TurnEndReturn && base.Zone == CardZone.Discard)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
				base.SetTurnCost(base.Mana);
			}
			this.TurnEndReturn = false;
			yield break;
		}
	}
}
