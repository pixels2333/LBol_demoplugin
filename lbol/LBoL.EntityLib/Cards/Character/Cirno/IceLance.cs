using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004C3 RID: 1219
	[UsedImplicitly]
	public sealed class IceLance : Card
	{
		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x0600102D RID: 4141 RVA: 0x0001CA70 File Offset: 0x0001AC70
		protected override int AdditionalValue1
		{
			get
			{
				if (base.Battle == null || !base.Battle.Player.HasStatusEffect<ColdHeartedSe>())
				{
					return 0;
				}
				return 1;
			}
		}

		// Token: 0x0600102E RID: 4142 RVA: 0x0001CA8F File Offset: 0x0001AC8F
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Battle.Player.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing), GameEventPriority.ConfigDefault);
		}

		// Token: 0x0600102F RID: 4143 RVA: 0x0001CAB8 File Offset: 0x0001ACB8
		private void OnPlayerDamageDealing(DamageDealingEventArgs args)
		{
			if (args.ActionSource == this && args.Targets != null)
			{
				if (Enumerable.Any<Unit>(args.Targets, (Unit target) => target.HasStatusEffect<Cold>()))
				{
					args.DamageInfo = args.DamageInfo.MultiplyBy(base.Value1);
					args.AddModifier(this);
				}
			}
		}

		// Token: 0x06001030 RID: 4144 RVA: 0x0001CB23 File Offset: 0x0001AD23
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			string text = base.GunName;
			if (base.Battle.Player.HasStatusEffect<ColdHeartedSe>())
			{
				text = "血脉" + base.GunName;
			}
			yield return base.AttackAction(selector, text);
			yield break;
		}
	}
}
