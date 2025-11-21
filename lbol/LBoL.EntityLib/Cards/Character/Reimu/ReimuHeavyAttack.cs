using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003F1 RID: 1009
	[UsedImplicitly]
	public sealed class ReimuHeavyAttack : Card
	{
		// Token: 0x17000191 RID: 401
		// (get) Token: 0x06000E08 RID: 3592 RVA: 0x0001A005 File Offset: 0x00018205
		private int PlayerTotalFire
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return base.Battle.Player.TotalFirepower;
			}
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x0001A021 File Offset: 0x00018221
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<DamageDealingEventArgs>(base.Battle.Player.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnPlayerDamageDealing), (GameEventPriority)0);
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x0001A048 File Offset: 0x00018248
		private void OnPlayerDamageDealing(DamageDealingEventArgs args)
		{
			if (args.ActionSource == this && args.DamageInfo.DamageType == DamageType.Attack && this.PlayerTotalFire > 0)
			{
				args.DamageInfo = args.DamageInfo.IncreaseBy(this.PlayerTotalFire * (base.Value1 - 1));
				args.AddModifier(this);
			}
		}
	}
}
