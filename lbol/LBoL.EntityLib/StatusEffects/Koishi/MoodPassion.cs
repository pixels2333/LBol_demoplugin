using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x0200007D RID: 125
	public sealed class MoodPassion : Mood
	{
		// Token: 0x060001B5 RID: 437 RVA: 0x000055E6 File Offset: 0x000037E6
		protected override string GetBaseDescription()
		{
			if (!this.HasLunatic)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x060001B6 RID: 438 RVA: 0x000055FD File Offset: 0x000037FD
		private bool HasLunatic
		{
			get
			{
				return base.Owner != null && base.Owner.HasStatusEffect<LunaticPassionSe>();
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060001B7 RID: 439 RVA: 0x00005614 File Offset: 0x00003814
		[UsedImplicitly]
		public int AttackPercentage
		{
			get
			{
				if (!this.HasLunatic)
				{
					return 100;
				}
				return 150;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060001B8 RID: 440 RVA: 0x00005626 File Offset: 0x00003826
		[UsedImplicitly]
		public int Percentage
		{
			get
			{
				return 100;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060001B9 RID: 441 RVA: 0x0000562A File Offset: 0x0000382A
		private float AttackRatio
		{
			get
			{
				return ((float)this.AttackPercentage + 100f) / 100f;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060001BA RID: 442 RVA: 0x0000563F File Offset: 0x0000383F
		private float Ratio
		{
			get
			{
				return ((float)this.Percentage + 100f) / 100f;
			}
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00005654 File Offset: 0x00003854
		protected override void OnAdded(Unit unit)
		{
			base.OnAdded(unit);
			base.HandleOwnerEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.HandleOwnerEvent<DamageEventArgs>(base.Owner.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
			if (base.Owner.HasStatusEffect<PassionDrawSe>())
			{
				int level = base.Owner.GetStatusEffect<PassionDrawSe>().Level;
				this.React(new DrawManyCardAction(level));
			}
		}

		// Token: 0x060001BC RID: 444 RVA: 0x000056D4 File Offset: 0x000038D4
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.MultiplyBy(this.AttackRatio);
				if (this.HasLunatic)
				{
					Card card = args.ActionSource as Card;
					if (card != null && card.Config.Type == CardType.Attack)
					{
						DamageInfo damageInfo = args.DamageInfo;
						damageInfo.IsAccuracy = true;
						args.DamageInfo = damageInfo;
					}
				}
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					base.NotifyActivating();
				}
			}
		}

		// Token: 0x060001BD RID: 445 RVA: 0x00005760 File Offset: 0x00003960
		private void OnDamageReceiving(DamageEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.MultiplyBy(this.Ratio);
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					base.NotifyActivating();
				}
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060001BE RID: 446 RVA: 0x000057AF File Offset: 0x000039AF
		public override string UnitEffectName
		{
			get
			{
				return "BenwoLoop";
			}
		}
	}
}
