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
	public sealed class MoodPassion : Mood
	{
		protected override string GetBaseDescription()
		{
			if (!this.HasLunatic)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		private bool HasLunatic
		{
			get
			{
				return base.Owner != null && base.Owner.HasStatusEffect<LunaticPassionSe>();
			}
		}
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
		[UsedImplicitly]
		public int Percentage
		{
			get
			{
				return 100;
			}
		}
		private float AttackRatio
		{
			get
			{
				return ((float)this.AttackPercentage + 100f) / 100f;
			}
		}
		private float Ratio
		{
			get
			{
				return ((float)this.Percentage + 100f) / 100f;
			}
		}
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
		public override string UnitEffectName
		{
			get
			{
				return "BenwoLoop";
			}
		}
	}
}
