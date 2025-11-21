using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Attributes;
using LBoL.Core.Intentions;

namespace LBoL.Core.Units
{
	// Token: 0x02000082 RID: 130
	[Localizable]
	public abstract class Intention : GameEntity
	{
		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000620 RID: 1568
		public abstract IntentionType Type { get; }

		// Token: 0x06000621 RID: 1569 RVA: 0x0001355A File Offset: 0x0001175A
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Intention>.LocalizeProperty(base.GetType().Name, key, decorated, required);
		}

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06000622 RID: 1570 RVA: 0x0001356F File Offset: 0x0001176F
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				throw new InvalidOperationException();
			}
		}

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06000623 RID: 1571 RVA: 0x00013576 File Offset: 0x00011776
		// (set) Token: 0x06000624 RID: 1572 RVA: 0x0001357E File Offset: 0x0001177E
		public EnemyUnit Source { get; private set; }

		// Token: 0x06000625 RID: 1573 RVA: 0x00013587 File Offset: 0x00011787
		internal Intention SetSource(EnemyUnit unit)
		{
			this.Source = unit;
			return this;
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x00013591 File Offset: 0x00011791
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new Intention.IntentionFormatWrapper(this);
		}

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06000627 RID: 1575 RVA: 0x00013599 File Offset: 0x00011799
		[UsedImplicitly]
		public override UnitName PlayerName
		{
			get
			{
				return this.Source.GameRun.Player.GetName();
			}
		}

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x06000628 RID: 1576 RVA: 0x000135B0 File Offset: 0x000117B0
		[UsedImplicitly]
		public UnitName OwnerName
		{
			get
			{
				return this.Source.GetName();
			}
		}

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06000629 RID: 1577 RVA: 0x000135BD File Offset: 0x000117BD
		// (set) Token: 0x0600062A RID: 1578 RVA: 0x000135C5 File Offset: 0x000117C5
		public string MoveName { get; private set; }

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x0600062B RID: 1579 RVA: 0x000135CE File Offset: 0x000117CE
		public bool HiddenFinal
		{
			get
			{
				return (this.HiddenByEnemy || this.Source.Battle.HideEnemyIntention) && !this.ShowByEnemyTurn;
			}
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x0600062C RID: 1580 RVA: 0x000135F5 File Offset: 0x000117F5
		// (set) Token: 0x0600062D RID: 1581 RVA: 0x000135FD File Offset: 0x000117FD
		public bool HiddenByEnemy { get; set; }

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x0600062E RID: 1582 RVA: 0x00013606 File Offset: 0x00011806
		// (set) Token: 0x0600062F RID: 1583 RVA: 0x0001360E File Offset: 0x0001180E
		public bool ShowByEnemyTurn { get; set; }

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06000630 RID: 1584 RVA: 0x00013617 File Offset: 0x00011817
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

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000631 RID: 1585 RVA: 0x0001363B File Offset: 0x0001183B
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

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x06000632 RID: 1586 RVA: 0x0001365F File Offset: 0x0001185F
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

		// Token: 0x06000633 RID: 1587 RVA: 0x0001368C File Offset: 0x0001188C
		public void AsHidden(bool hidden = true)
		{
			this.HiddenByEnemy = hidden;
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x00013695 File Offset: 0x00011895
		public Intention WithMoveName(string moveName)
		{
			this.MoveName = moveName;
			return this;
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x000136A0 File Offset: 0x000118A0
		protected int CalculateDamage(DamageInfo damage)
		{
			EnemyUnit source = this.Source;
			return source.Battle.CalculateDamage(source, source, source.Battle.Player, damage);
		}

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x06000636 RID: 1590 RVA: 0x000136D0 File Offset: 0x000118D0
		// (remove) Token: 0x06000637 RID: 1591 RVA: 0x00013708 File Offset: 0x00011908
		public event Action Activating;

		// Token: 0x06000638 RID: 1592 RVA: 0x0001373D File Offset: 0x0001193D
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0001374F File Offset: 0x0001194F
		public static Intention Attack(int damage, int times, bool isAccuracy = false)
		{
			AttackIntention attackIntention = TypeFactory<Intention>.CreateInstance<AttackIntention>();
			attackIntention.Damage = DamageInfo.Attack((float)damage, isAccuracy);
			attackIntention.Times = new int?(times);
			attackIntention.IsAccuracy = isAccuracy;
			return attackIntention;
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x00013777 File Offset: 0x00011977
		public static Intention Attack(int damage, bool isAccuracy = false)
		{
			AttackIntention attackIntention = TypeFactory<Intention>.CreateInstance<AttackIntention>();
			attackIntention.Damage = DamageInfo.Attack((float)damage, isAccuracy);
			attackIntention.IsAccuracy = isAccuracy;
			return attackIntention;
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x00013793 File Offset: 0x00011993
		public static Intention KokoroDark(int damage, int count)
		{
			KokoroDarkIntention kokoroDarkIntention = TypeFactory<Intention>.CreateInstance<KokoroDarkIntention>();
			kokoroDarkIntention.Damage = DamageInfo.Reaction((float)damage, false);
			kokoroDarkIntention.Count = count;
			return kokoroDarkIntention;
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x000137AF File Offset: 0x000119AF
		public static Intention Defend()
		{
			return TypeFactory<Intention>.CreateInstance<DefendIntention>();
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x000137B6 File Offset: 0x000119B6
		public static Intention Graze()
		{
			return TypeFactory<Intention>.CreateInstance<GrazeIntention>();
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x000137BD File Offset: 0x000119BD
		public static Intention PositiveEffect()
		{
			return TypeFactory<Intention>.CreateInstance<PositiveEffectIntention>();
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x000137C4 File Offset: 0x000119C4
		public static Intention NegativeEffect(string specialIconName = null)
		{
			NegativeEffectIntention negativeEffectIntention = TypeFactory<Intention>.CreateInstance<NegativeEffectIntention>();
			if (!specialIconName.IsNullOrEmpty())
			{
				negativeEffectIntention.SpecialIconName = specialIconName;
			}
			return negativeEffectIntention;
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x000137E7 File Offset: 0x000119E7
		public static Intention Spawn()
		{
			return TypeFactory<Intention>.CreateInstance<SpawnIntention>();
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x000137EE File Offset: 0x000119EE
		public static Intention SpawnDrone()
		{
			return TypeFactory<Intention>.CreateInstance<SpawnDroneIntention>();
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x000137F5 File Offset: 0x000119F5
		public static Intention Sleep()
		{
			return TypeFactory<Intention>.CreateInstance<SleepIntention>();
		}

		// Token: 0x06000643 RID: 1603 RVA: 0x000137FC File Offset: 0x000119FC
		public static Intention Stun()
		{
			return TypeFactory<Intention>.CreateInstance<StunIntention>();
		}

		// Token: 0x06000644 RID: 1604 RVA: 0x00013803 File Offset: 0x00011A03
		public static Intention Escape()
		{
			return TypeFactory<Intention>.CreateInstance<EscapeIntention>();
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x0001380A File Offset: 0x00011A0A
		public static Intention Explode(int damage)
		{
			ExplodeIntention explodeIntention = TypeFactory<Intention>.CreateInstance<ExplodeIntention>();
			explodeIntention.Damage = DamageInfo.Attack((float)damage, false);
			return explodeIntention;
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x0001381F File Offset: 0x00011A1F
		public static Intention ExplodeAlly()
		{
			return TypeFactory<Intention>.CreateInstance<ExplodeAllyIntention>();
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x00013826 File Offset: 0x00011A26
		public static Intention Charge()
		{
			return TypeFactory<Intention>.CreateInstance<ChargeIntention>();
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x0001382D File Offset: 0x00011A2D
		public static Intention AddCard()
		{
			return TypeFactory<Intention>.CreateInstance<AddCardIntention>();
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x00013834 File Offset: 0x00011A34
		public static Intention Heal()
		{
			return TypeFactory<Intention>.CreateInstance<HealIntention>();
		}

		// Token: 0x0600064A RID: 1610 RVA: 0x0001383B File Offset: 0x00011A3B
		public static Intention Repair()
		{
			return TypeFactory<Intention>.CreateInstance<RepairIntention>();
		}

		// Token: 0x0600064B RID: 1611 RVA: 0x00013842 File Offset: 0x00011A42
		public static Intention Clear()
		{
			return TypeFactory<Intention>.CreateInstance<ClearIntention>();
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x00013849 File Offset: 0x00011A49
		public static Intention Hex()
		{
			return TypeFactory<Intention>.CreateInstance<HexIntention>();
		}

		// Token: 0x0600064D RID: 1613 RVA: 0x00013850 File Offset: 0x00011A50
		public static Intention CountDown(int counter)
		{
			CountDownIntention countDownIntention = TypeFactory<Intention>.CreateInstance<CountDownIntention>();
			countDownIntention.Counter = counter;
			return countDownIntention;
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x00013860 File Offset: 0x00011A60
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

		// Token: 0x0600064F RID: 1615 RVA: 0x000138B4 File Offset: 0x00011AB4
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

		// Token: 0x06000650 RID: 1616 RVA: 0x0001390C File Offset: 0x00011B0C
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

		// Token: 0x06000651 RID: 1617 RVA: 0x0001396D File Offset: 0x00011B6D
		public static Intention Unknown()
		{
			return TypeFactory<Intention>.CreateInstance<UnknownIntention>();
		}

		// Token: 0x06000652 RID: 1618 RVA: 0x00013974 File Offset: 0x00011B74
		public static Intention DoNothing()
		{
			return TypeFactory<Intention>.CreateInstance<DoNothingIntention>();
		}

		// Token: 0x040002E4 RID: 740
		public string SpecialIconName;

		// Token: 0x040002E7 RID: 743
		private Intention _hiddenUnknownIntention;

		// Token: 0x02000230 RID: 560
		internal sealed class IntentionFormatWrapper : GameEntityFormatWrapper
		{
			// Token: 0x060011C4 RID: 4548 RVA: 0x00030603 File Offset: 0x0002E803
			public IntentionFormatWrapper(Intention intention)
				: base(intention)
			{
				this._intention = intention;
			}

			// Token: 0x060011C5 RID: 4549 RVA: 0x00030614 File Offset: 0x0002E814
			protected override string FormatArgument(object arg, string format)
			{
				if (arg is DamageInfo)
				{
					DamageInfo damageInfo = (DamageInfo)arg;
					return base.FormatArgument(this._intention.CalculateDamage(damageInfo), format);
				}
				return base.FormatArgument(arg, format);
			}

			// Token: 0x040008B8 RID: 2232
			private readonly Intention _intention;
		}
	}
}
