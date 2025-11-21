using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.Core.Units
{
	// Token: 0x02000089 RID: 137
	[Localizable]
	public abstract class UltimateSkill : GameEntity
	{
		// Token: 0x06000692 RID: 1682 RVA: 0x000141A9 File Offset: 0x000123A9
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<UltimateSkill>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x06000693 RID: 1683 RVA: 0x000141B9 File Offset: 0x000123B9
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<UltimateSkill>.LocalizeListProperty(base.Id, key, required);
		}

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06000694 RID: 1684 RVA: 0x000141C8 File Offset: 0x000123C8
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x06000695 RID: 1685 RVA: 0x000141D5 File Offset: 0x000123D5
		// (set) Token: 0x06000696 RID: 1686 RVA: 0x000141DD File Offset: 0x000123DD
		public UltimateSkillConfig Config { get; private set; }

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x06000697 RID: 1687 RVA: 0x000141E8 File Offset: 0x000123E8
		public override string DebugName
		{
			get
			{
				return string.Concat(new string[] { "<", base.Id, ">(", this.Title, "-", this.Content, ")" });
			}
		}

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x06000698 RID: 1688 RVA: 0x0001423B File Offset: 0x0001243B
		public override string Name
		{
			get
			{
				return this.Title + "-" + this.Content;
			}
		}

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06000699 RID: 1689 RVA: 0x00014254 File Offset: 0x00012454
		// (set) Token: 0x0600069A RID: 1690 RVA: 0x00014273 File Offset: 0x00012473
		protected internal PlayerUnit Owner
		{
			get
			{
				PlayerUnit playerUnit;
				if (!this._owner.TryGetTarget(ref playerUnit))
				{
					return null;
				}
				return playerUnit;
			}
			set
			{
				this._owner.SetTarget(value);
			}
		}

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x0600069B RID: 1691 RVA: 0x00014281 File Offset: 0x00012481
		public BattleController Battle
		{
			get
			{
				PlayerUnit owner = this.Owner;
				if (owner == null)
				{
					return null;
				}
				return owner.Battle;
			}
		}

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x0600069C RID: 1692 RVA: 0x00014294 File Offset: 0x00012494
		public int PowerCost
		{
			get
			{
				return this.Config.PowerCost;
			}
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x0600069D RID: 1693 RVA: 0x000142A1 File Offset: 0x000124A1
		public bool TurnRepeatable
		{
			get
			{
				return this.UsRepeatableType == UsRepeatableType.FreeToUse;
			}
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x0600069E RID: 1694 RVA: 0x000142AC File Offset: 0x000124AC
		public bool BattleRepeatable
		{
			get
			{
				UsRepeatableType usRepeatableType = this.UsRepeatableType;
				return usRepeatableType == UsRepeatableType.FreeToUse || usRepeatableType == UsRepeatableType.OncePerTurn;
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x0600069F RID: 1695 RVA: 0x000142D0 File Offset: 0x000124D0
		// (set) Token: 0x060006A0 RID: 1696 RVA: 0x000142D8 File Offset: 0x000124D8
		public UsRepeatableType UsRepeatableType { get; set; }

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x060006A1 RID: 1697 RVA: 0x000142E1 File Offset: 0x000124E1
		// (set) Token: 0x060006A2 RID: 1698 RVA: 0x000142E9 File Offset: 0x000124E9
		public string GunName { get; protected set; }

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x060006A3 RID: 1699 RVA: 0x000142F2 File Offset: 0x000124F2
		public int PowerPerLevel
		{
			get
			{
				return this.Config.PowerPerLevel;
			}
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x060006A4 RID: 1700 RVA: 0x000142FF File Offset: 0x000124FF
		// (set) Token: 0x060006A5 RID: 1701 RVA: 0x00014307 File Offset: 0x00012507
		public int MaxPowerLevel { get; set; }

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x060006A6 RID: 1702 RVA: 0x00014310 File Offset: 0x00012510
		[UsedImplicitly]
		public virtual DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)this.Config.Damage, true);
			}
		}

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x060006A7 RID: 1703 RVA: 0x00014324 File Offset: 0x00012524
		[UsedImplicitly]
		public int Value1
		{
			get
			{
				return this.Config.Value1;
			}
		}

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x060006A8 RID: 1704 RVA: 0x00014331 File Offset: 0x00012531
		[UsedImplicitly]
		public int Value2
		{
			get
			{
				return this.Config.Value2;
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x060006A9 RID: 1705 RVA: 0x0001433E File Offset: 0x0001253E
		public string Title
		{
			get
			{
				return this.LocalizeProperty("Title", false, false);
			}
		}

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x060006AA RID: 1706 RVA: 0x0001434D File Offset: 0x0001254D
		public string Content
		{
			get
			{
				return this.LocalizeProperty("Content", false, false);
			}
		}

		// Token: 0x1700022A RID: 554
		// (get) Token: 0x060006AB RID: 1707 RVA: 0x0001435C File Offset: 0x0001255C
		// (set) Token: 0x060006AC RID: 1708 RVA: 0x00014364 File Offset: 0x00012564
		public bool TurnAvailable { get; internal set; }

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x060006AD RID: 1709 RVA: 0x0001436D File Offset: 0x0001256D
		// (set) Token: 0x060006AE RID: 1710 RVA: 0x00014375 File Offset: 0x00012575
		public bool BattleAvailable { get; internal set; }

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x060006AF RID: 1711 RVA: 0x0001437E File Offset: 0x0001257E
		public bool Available
		{
			get
			{
				return this.TurnAvailable && this.BattleAvailable;
			}
		}

		// Token: 0x1700022D RID: 557
		// (get) Token: 0x060006B0 RID: 1712 RVA: 0x00014390 File Offset: 0x00012590
		// (set) Token: 0x060006B1 RID: 1713 RVA: 0x00014398 File Offset: 0x00012598
		public TargetType TargetType { get; protected set; }

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x060006B2 RID: 1714 RVA: 0x000143A1 File Offset: 0x000125A1
		// (set) Token: 0x060006B3 RID: 1715 RVA: 0x000143A9 File Offset: 0x000125A9
		public Unit PendingTarget
		{
			get
			{
				return this._pendingTarget;
			}
			set
			{
				if (value != this._pendingTarget)
				{
					this._pendingTarget = value;
					this.NotifyChanged();
				}
			}
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x000143C4 File Offset: 0x000125C4
		public override void Initialize()
		{
			base.Initialize();
			this.Config = UltimateSkillConfig.FromId(base.Id);
			this.UsRepeatableType = this.Config.RepeatableType;
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find ultimate-skill config for <" + base.Id + ">");
			}
			this.MaxPowerLevel = this.Config.MaxPowerLevel;
		}

		// Token: 0x060006B5 RID: 1717 RVA: 0x0001442D File Offset: 0x0001262D
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new UltimateSkill.UsFormatWrapper(this);
		}

		// Token: 0x060006B6 RID: 1718 RVA: 0x00014435 File Offset: 0x00012635
		internal IEnumerable<BattleAction> GetActions(UnitSelector selector, IList<DamageAction> damageActions)
		{
			if (selector.Type == TargetType.SingleEnemy)
			{
				this.PendingTarget = selector.SelectedEnemy;
			}
			foreach (BattleAction battleAction in this.Actions(selector))
			{
				DamageAction damageAction = battleAction as DamageAction;
				if (damageAction != null)
				{
					damageActions.Add(damageAction);
				}
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			this.PendingTarget = null;
			yield break;
			yield break;
		}

		// Token: 0x060006B7 RID: 1719
		protected abstract IEnumerable<BattleAction> Actions(UnitSelector selector);

		// Token: 0x060006B8 RID: 1720 RVA: 0x00014454 File Offset: 0x00012654
		public IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Config.Keywords, this.Config.RelativeEffects, verbose, default(Keyword?));
		}

		// Token: 0x060006B9 RID: 1721 RVA: 0x0001448C File Offset: 0x0001268C
		public IEnumerable<Card> EnumerateRelativeCards()
		{
			IReadOnlyList<string> readOnlyList = this.Config.RelativeCards ?? Array.Empty<string>();
			foreach (string text in readOnlyList)
			{
				Card card;
				if (Enumerable.Last<char>(text) == '+')
				{
					string text2 = text;
					card = Library.CreateCard(text2.Substring(0, text2.Length - 1));
					card.Upgrade();
				}
				else
				{
					card = Library.CreateCard(text);
				}
				card.GameRun = base.GameRun;
				yield return card;
			}
			IEnumerator<string> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x04000311 RID: 785
		private readonly WeakReference<PlayerUnit> _owner = new WeakReference<PlayerUnit>(null);

		// Token: 0x04000318 RID: 792
		private Unit _pendingTarget;

		// Token: 0x02000234 RID: 564
		internal sealed class UsFormatWrapper : GameEntityFormatWrapper
		{
			// Token: 0x060011CC RID: 4556 RVA: 0x000306A2 File Offset: 0x0002E8A2
			public UsFormatWrapper(UltimateSkill us)
				: base(us)
			{
				this._us = us;
			}

			// Token: 0x060011CD RID: 4557 RVA: 0x000306B4 File Offset: 0x0002E8B4
			protected override string FormatArgument(object arg, string format)
			{
				if (arg is DamageInfo)
				{
					DamageInfo damageInfo = (DamageInfo)arg;
					int num = (int)damageInfo.Damage;
					if (this._us.Battle == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(num, num, format);
					}
					int num2 = this._us.Battle.CalculateDamage(this._us, this._us.Battle.Player, this._us.PendingTarget, damageInfo);
					return GameEntityFormatWrapper.WrappedFormatNumber(num, num2, format);
				}
				else if (arg is BlockInfo)
				{
					int block = ((BlockInfo)arg).Block;
					if (this._us.Battle == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(block, block, format);
					}
					int item = this._us.Battle.CalculateBlockShield(this._us, (float)block, 0f, BlockShieldType.Normal).Item1;
					return GameEntityFormatWrapper.WrappedFormatNumber(block, item, format);
				}
				else
				{
					if (!(arg is ShieldInfo))
					{
						return base.FormatArgument(arg, format);
					}
					ShieldInfo shieldInfo = (ShieldInfo)arg;
					int shield = shieldInfo.Shield;
					if (this._us.Battle == null)
					{
						return GameEntityFormatWrapper.WrappedFormatNumber(shield, shield, format);
					}
					int item2 = this._us.Battle.CalculateBlockShield(this._us, 0f, (float)shieldInfo.Shield, BlockShieldType.Normal).Item2;
					return GameEntityFormatWrapper.WrappedFormatNumber(shield, item2, format);
				}
			}

			// Token: 0x040008BC RID: 2236
			private readonly UltimateSkill _us;
		}
	}
}
