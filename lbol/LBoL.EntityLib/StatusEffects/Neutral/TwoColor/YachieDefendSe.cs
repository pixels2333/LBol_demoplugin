using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x0200004D RID: 77
	[UsedImplicitly]
	public sealed class YachieDefendSe : StatusEffect
	{
		// Token: 0x17000011 RID: 17
		// (get) Token: 0x060000F4 RID: 244 RVA: 0x00003B4A File Offset: 0x00001D4A
		// (set) Token: 0x060000F5 RID: 245 RVA: 0x00003B52 File Offset: 0x00001D52
		private Queue<ReflectDamage> ReflectDamages { get; set; } = new Queue<ReflectDamage>();

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x060000F6 RID: 246 RVA: 0x00003B5B File Offset: 0x00001D5B
		// (set) Token: 0x060000F7 RID: 247 RVA: 0x00003B63 File Offset: 0x00001D63
		private int ActiveTimes { get; set; }

		// Token: 0x060000F8 RID: 248 RVA: 0x00003B6C File Offset: 0x00001D6C
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
			base.ReactOwnerEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived));
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00003BC0 File Offset: 0x00001DC0
		private void OnPlayerDamageTaking(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			int num = damageInfo.Damage.RoundToInt();
			if (num >= 1 && this.ActiveTimes < base.Level)
			{
				base.NotifyActivating();
				int num2 = this.ActiveTimes + 1;
				this.ActiveTimes = num2;
				args.DamageInfo = damageInfo.ReduceActualDamageBy(num);
				args.AddModifier(this);
				Unit source = args.Source;
				if (source is EnemyUnit && source.IsAlive)
				{
					this.ReflectDamages.Enqueue(new ReflectDamage(args.Source, num));
				}
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00003C4C File Offset: 0x00001E4C
		private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
		{
			while (this.ReflectDamages.Count > 0)
			{
				ReflectDamage reflectDamage = this.ReflectDamages.Dequeue();
				if (reflectDamage.Target.IsAlive)
				{
					yield return new DamageAction(base.Battle.Player, reflectDamage.Target, DamageInfo.Reaction((float)reflectDamage.Damage, false), "Instant", GunType.Single);
				}
			}
			base.Level -= this.ActiveTimes;
			this.ActiveTimes = 0;
			if (base.Level <= 0)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
