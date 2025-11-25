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
	[UsedImplicitly]
	public sealed class YachieDefendSe : StatusEffect
	{
		private Queue<ReflectDamage> ReflectDamages { get; set; } = new Queue<ReflectDamage>();
		private int ActiveTimes { get; set; }
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
			base.ReactOwnerEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived));
		}
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
