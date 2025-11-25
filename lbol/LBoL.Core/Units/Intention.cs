using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Attributes;
using LBoL.Core.Intentions;
namespace LBoL.Core.Units
{
	[Localizable]
	public abstract class Intention : GameEntity
	{
		public abstract IntentionType Type { get; }
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Intention>.LocalizeProperty(base.GetType().Name, key, decorated, required);
		}
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				throw new InvalidOperationException();
			}
		}
		public EnemyUnit Source { get; private set; }
		internal Intention SetSource(EnemyUnit unit)
		{
			this.Source = unit;
			return this;
		}
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new Intention.IntentionFormatWrapper(this);
		}
		[UsedImplicitly]
		public override UnitName PlayerName
		{
			get
			{
				return this.Source.GameRun.Player.GetName();
			}
		}
		[UsedImplicitly]
		public UnitName OwnerName
		{
			get
			{
				return this.Source.GetName();
			}
		}
		public string MoveName { get; private set; }
		public bool HiddenFinal
		{
			get
			{
				return (this.HiddenByEnemy || this.Source.Battle.HideEnemyIntention) && !this.ShowByEnemyTurn;
			}
		}
		public bool HiddenByEnemy { get; set; }
		public bool ShowByEnemyTurn { get; set; }
		public override string Name
		{
			get
			{
				if (!this.HiddenFinal || this is UnknownIntention)
				{
					return base.Name;
				}
				return this.HiddenUnknownIntention.Name;
			}
		}
		public override string Description
		{
			get
			{
				if (!this.HiddenFinal || this is UnknownIntention)
				{
					return base.Description;
				}
				return this.HiddenUnknownIntention.Description;
			}
		}
		private Intention HiddenUnknownIntention
		{
			get
			{
				if (this._hiddenUnknownIntention == null)
				{
					this._hiddenUnknownIntention = Library.CreateIntention<UnknownIntention>();
					this._hiddenUnknownIntention.SetSource(this.Source);
				}
				return this._hiddenUnknownIntention;
			}
		}
		public void AsHidden(bool hidden = true)
		{
			this.HiddenByEnemy = hidden;
		}
		public Intention WithMoveName(string moveName)
		{
			this.MoveName = moveName;
			return this;
		}
		protected int CalculateDamage(DamageInfo damage)
		{
			EnemyUnit source = this.Source;
			return source.Battle.CalculateDamage(source, source, source.Battle.Player, damage);
		}
		public event Action Activating;
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}
		public static Intention Attack(int damage, int times, bool isAccuracy = false)
		{
			AttackIntention attackIntention = TypeFactory<Intention>.CreateInstance<AttackIntention>();
			attackIntention.Damage = DamageInfo.Attack((float)damage, isAccuracy);
			attackIntention.Times = new int?(times);
			attackIntention.IsAccuracy = isAccuracy;
			return attackIntention;
		}
		public static Intention Attack(int damage, bool isAccuracy = false)
		{
			AttackIntention attackIntention = TypeFactory<Intention>.CreateInstance<AttackIntention>();
			attackIntention.Damage = DamageInfo.Attack((float)damage, isAccuracy);
			attackIntention.IsAccuracy = isAccuracy;
			return attackIntention;
		}
		public static Intention KokoroDark(int damage, int count)
		{
			KokoroDarkIntention kokoroDarkIntention = TypeFactory<Intention>.CreateInstance<KokoroDarkIntention>();
			kokoroDarkIntention.Damage = DamageInfo.Reaction((float)damage, false);
			kokoroDarkIntention.Count = count;
			return kokoroDarkIntention;
		}
		public static Intention Defend()
		{
			return TypeFactory<Intention>.CreateInstance<DefendIntention>();
		}
		public static Intention Graze()
		{
			return TypeFactory<Intention>.CreateInstance<GrazeIntention>();
		}
		public static Intention PositiveEffect()
		{
			return TypeFactory<Intention>.CreateInstance<PositiveEffectIntention>();
		}
		public static Intention NegativeEffect(string specialIconName = null)
		{
			NegativeEffectIntention negativeEffectIntention = TypeFactory<Intention>.CreateInstance<NegativeEffectIntention>();
			if (!specialIconName.IsNullOrEmpty())
			{
				negativeEffectIntention.SpecialIconName = specialIconName;
			}
			return negativeEffectIntention;
		}
		public static Intention Spawn()
		{
			return TypeFactory<Intention>.CreateInstance<SpawnIntention>();
		}
		public static Intention SpawnDrone()
		{
			return TypeFactory<Intention>.CreateInstance<SpawnDroneIntention>();
		}
		public static Intention Sleep()
		{
			return TypeFactory<Intention>.CreateInstance<SleepIntention>();
		}
		public static Intention Stun()
		{
			return TypeFactory<Intention>.CreateInstance<StunIntention>();
		}
		public static Intention Escape()
		{
			return TypeFactory<Intention>.CreateInstance<EscapeIntention>();
		}
		public static Intention Explode(int damage)
		{
			ExplodeIntention explodeIntention = TypeFactory<Intention>.CreateInstance<ExplodeIntention>();
			explodeIntention.Damage = DamageInfo.Attack((float)damage, false);
			return explodeIntention;
		}
		public static Intention ExplodeAlly()
		{
			return TypeFactory<Intention>.CreateInstance<ExplodeAllyIntention>();
		}
		public static Intention Charge()
		{
			return TypeFactory<Intention>.CreateInstance<ChargeIntention>();
		}
		public static Intention AddCard()
		{
			return TypeFactory<Intention>.CreateInstance<AddCardIntention>();
		}
		public static Intention Heal()
		{
			return TypeFactory<Intention>.CreateInstance<HealIntention>();
		}
		public static Intention Repair()
		{
			return TypeFactory<Intention>.CreateInstance<RepairIntention>();
		}
		public static Intention Clear()
		{
			return TypeFactory<Intention>.CreateInstance<ClearIntention>();
		}
		public static Intention Hex()
		{
			return TypeFactory<Intention>.CreateInstance<HexIntention>();
		}
		public static Intention CountDown(int counter)
		{
			CountDownIntention countDownIntention = TypeFactory<Intention>.CreateInstance<CountDownIntention>();
			countDownIntention.Counter = counter;
			return countDownIntention;
		}
		public static Intention SpellCard(string name, int? damage, bool isAccuracy)
		{
			SpellCardIntention spellCardIntention = TypeFactory<Intention>.CreateInstance<SpellCardIntention>();
			spellCardIntention.MoveName = name;
			DamageInfo? damageInfo;
			if (damage != null)
			{
				int valueOrDefault = damage.GetValueOrDefault();
				damageInfo = new DamageInfo?(DamageInfo.Attack((float)valueOrDefault, isAccuracy));
			}
			else
			{
				damageInfo = default(DamageInfo?);
			}
			spellCardIntention.Damage = damageInfo;
			spellCardIntention.IsAccuracy = isAccuracy;
			return spellCardIntention;
		}
		public static Intention SpellCard(string name, int? damage = null, int? times = null, bool isAccuracy = false)
		{
			SpellCardIntention spellCardIntention = TypeFactory<Intention>.CreateInstance<SpellCardIntention>();
			spellCardIntention.MoveName = name;
			DamageInfo? damageInfo;
			if (damage != null)
			{
				int valueOrDefault = damage.GetValueOrDefault();
				damageInfo = new DamageInfo?(DamageInfo.Attack((float)valueOrDefault, isAccuracy));
			}
			else
			{
				damageInfo = default(DamageInfo?);
			}
			spellCardIntention.Damage = damageInfo;
			spellCardIntention.Times = times;
			spellCardIntention.IsAccuracy = isAccuracy;
			return spellCardIntention;
		}
		public static Intention SpellCard(string name, string iconName, int? damage = null, int? times = null, bool isAccuracy = false)
		{
			SpellCardIntention spellCardIntention = TypeFactory<Intention>.CreateInstance<SpellCardIntention>();
			spellCardIntention.MoveName = name;
			DamageInfo? damageInfo;
			if (damage != null)
			{
				int valueOrDefault = damage.GetValueOrDefault();
				damageInfo = new DamageInfo?(DamageInfo.Attack((float)valueOrDefault, isAccuracy));
			}
			else
			{
				damageInfo = default(DamageInfo?);
			}
			spellCardIntention.Damage = damageInfo;
			spellCardIntention.Times = times;
			spellCardIntention.IconName = iconName;
			spellCardIntention.IsAccuracy = isAccuracy;
			return spellCardIntention;
		}
		public static Intention Unknown()
		{
			return TypeFactory<Intention>.CreateInstance<UnknownIntention>();
		}
		public static Intention DoNothing()
		{
			return TypeFactory<Intention>.CreateInstance<DoNothingIntention>();
		}
		public string SpecialIconName;
		private Intention _hiddenUnknownIntention;
		internal sealed class IntentionFormatWrapper : GameEntityFormatWrapper
		{
			public IntentionFormatWrapper(Intention intention)
				: base(intention)
			{
				this._intention = intention;
			}
			protected override string FormatArgument(object arg, string format)
			{
				if (arg is DamageInfo)
				{
					DamageInfo damageInfo = (DamageInfo)arg;
					return base.FormatArgument(this._intention.CalculateDamage(damageInfo), format);
				}
				return base.FormatArgument(arg, format);
			}
			private readonly Intention _intention;
		}
	}
}
