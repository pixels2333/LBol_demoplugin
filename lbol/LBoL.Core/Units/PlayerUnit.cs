using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.StatusEffects;
using UnityEngine;

namespace LBoL.Core.Units
{
	// Token: 0x02000087 RID: 135
	[Localizable]
	public abstract class PlayerUnit : Unit
	{
		// Token: 0x170001FF RID: 511
		// (get) Token: 0x06000654 RID: 1620 RVA: 0x00013983 File Offset: 0x00011B83
		public bool IsSuperExtraTurn
		{
			get
			{
				return base.IsExtraTurn && base.HasStatusEffect<SuperExtraTurn>() && base.GetStatusEffect<SuperExtraTurn>().IsInExtraTurnByThis;
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000655 RID: 1621 RVA: 0x000139A4 File Offset: 0x00011BA4
		// (set) Token: 0x06000656 RID: 1622 RVA: 0x000139AC File Offset: 0x00011BAC
		public PlayerUnitConfig Config { get; private set; }

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000657 RID: 1623 RVA: 0x000139B5 File Offset: 0x00011BB5
		public string ModelName
		{
			get
			{
				if (!this.Config.ModleName.IsNullOrEmpty())
				{
					return this.Config.ModleName;
				}
				return base.Id;
			}
		}

		// Token: 0x06000658 RID: 1624 RVA: 0x000139DB File Offset: 0x00011BDB
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<PlayerUnit>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000659 RID: 1625 RVA: 0x000139EB File Offset: 0x00011BEB
		public string Title
		{
			get
			{
				return this.LocalizeProperty("Title", false, true);
			}
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x000139FA File Offset: 0x00011BFA
		public override UnitName GetName()
		{
			return UnitNameTable.GetName(base.Id, this.Config.NarrativeColor);
		}

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x0600065B RID: 1627 RVA: 0x00013A12 File Offset: 0x00011C12
		public bool HasHomeName
		{
			get
			{
				return this.Config.HasHomeName;
			}
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x00013A1F File Offset: 0x00011C1F
		public string GetHomeName()
		{
			return string.Concat(new string[]
			{
				"<color=",
				this.Config.NarrativeColor,
				">",
				this.HomeName,
				"</color>"
			});
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x0600065D RID: 1629 RVA: 0x00013A5B File Offset: 0x00011C5B
		public string HomeName
		{
			get
			{
				return this.LocalizeProperty("HomeName", false, true);
			}
		}

		// Token: 0x0600065E RID: 1630 RVA: 0x00013A6C File Offset: 0x00011C6C
		public override void Initialize()
		{
			base.Initialize();
			this.Config = PlayerUnitConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find player-unit config for <" + base.Id + ">");
			}
			base.SetMaxHp(this.Config.MaxHp, this.Config.MaxHp);
			this.Power = this.Config.InitialPower;
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x0600065F RID: 1631 RVA: 0x00013AE0 File Offset: 0x00011CE0
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06000660 RID: 1632 RVA: 0x00013AED File Offset: 0x00011CED
		public bool HasUs
		{
			get
			{
				return this.Us != null;
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x06000661 RID: 1633 RVA: 0x00013AF8 File Offset: 0x00011CF8
		// (set) Token: 0x06000662 RID: 1634 RVA: 0x00013B00 File Offset: 0x00011D00
		public UltimateSkill Us { get; private set; }

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x06000663 RID: 1635 RVA: 0x00013B09 File Offset: 0x00011D09
		// (set) Token: 0x06000664 RID: 1636 RVA: 0x00013B11 File Offset: 0x00011D11
		public int Power { get; internal set; }

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x06000665 RID: 1637 RVA: 0x00013B1A File Offset: 0x00011D1A
		public int PowerLevel
		{
			get
			{
				return this.Power / this.PowerPerLevel;
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000666 RID: 1638 RVA: 0x00013B29 File Offset: 0x00011D29
		public int PowerRemainder
		{
			get
			{
				return this.Power % this.PowerPerLevel;
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000667 RID: 1639 RVA: 0x00013B38 File Offset: 0x00011D38
		public int PowerPerLevel
		{
			get
			{
				UltimateSkill us = this.Us;
				if (us == null)
				{
					throw new InvalidOperationException(string.Concat(new string[] { "Player ", this.Name, "(", base.Id, ") has no US" }));
				}
				return us.PowerPerLevel;
			}
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000668 RID: 1640 RVA: 0x00013B90 File Offset: 0x00011D90
		public int MaxPowerLevel
		{
			get
			{
				UltimateSkill us = this.Us;
				if (us == null)
				{
					throw new InvalidOperationException(string.Concat(new string[] { "Player ", this.Name, "(", base.Id, ") has no US" }));
				}
				return us.MaxPowerLevel;
			}
		}

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000669 RID: 1641 RVA: 0x00013BE7 File Offset: 0x00011DE7
		public int MaxPower
		{
			get
			{
				return this.MaxPowerLevel * this.PowerPerLevel;
			}
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x00013BF8 File Offset: 0x00011DF8
		internal int GainPower(int power)
		{
			if (power < 0)
			{
				throw new InvalidOperationException(string.Format("Cannot gain negative power = {0}", power));
			}
			int num = Math.Min(this.MaxPower - this.Power, power);
			this.Power += num;
			return num;
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x00013C44 File Offset: 0x00011E44
		internal void ConsumePower(int power)
		{
			if (power < 0)
			{
				throw new InvalidOperationException(string.Format("Cannot consume negative power = {0}", power));
			}
			if (power > this.Power)
			{
				throw new InvalidCastException(string.Format("Cannot afford energy {0} with power = {1}", power, this.Power));
			}
			this.Power -= power;
		}

		// Token: 0x0600066C RID: 1644 RVA: 0x00013CA4 File Offset: 0x00011EA4
		internal int LosePower(int power)
		{
			if (power < 0)
			{
				throw new InvalidOperationException(string.Format("Cannot lose negative power = {0}", power));
			}
			int num = Math.Min(this.Power, power);
			this.Power -= num;
			return num;
		}

		// Token: 0x0600066D RID: 1645 RVA: 0x00013CE7 File Offset: 0x00011EE7
		public void SetUs(UltimateSkill us)
		{
			if (this.Us != null)
			{
				throw new InvalidOperationException(this.Name + " already has US " + this.Us.Name);
			}
			this.Us = us;
			us.Owner = this;
		}

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x0600066E RID: 1646 RVA: 0x00013D20 File Offset: 0x00011F20
		public IReadOnlyList<Exhibit> Exhibits
		{
			get
			{
				return this._exhibits.AsReadOnly();
			}
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x00013D30 File Offset: 0x00011F30
		public Exhibit GetExhibit(string id)
		{
			return Enumerable.FirstOrDefault<Exhibit>(this._exhibits, (Exhibit exhibit) => exhibit.Id == id);
		}

		// Token: 0x06000670 RID: 1648 RVA: 0x00013D64 File Offset: 0x00011F64
		public Exhibit GetExhibit(Type type)
		{
			return Enumerable.FirstOrDefault<Exhibit>(this._exhibits, (Exhibit exhibit) => exhibit.GetType() == type);
		}

		// Token: 0x06000671 RID: 1649 RVA: 0x00013D95 File Offset: 0x00011F95
		public T GetExhibit<T>() where T : Exhibit
		{
			return (T)((object)this.GetExhibit(typeof(T)));
		}

		// Token: 0x06000672 RID: 1650 RVA: 0x00013DAC File Offset: 0x00011FAC
		public bool HasExhibit(string id)
		{
			Type type = TypeFactory<Exhibit>.GetType(id);
			if (type != null)
			{
				return this.HasExhibit(type);
			}
			Debug.LogError("Cannot find exhibit type '" + id + "'");
			return false;
		}

		// Token: 0x06000673 RID: 1651 RVA: 0x00013DE8 File Offset: 0x00011FE8
		public bool HasExhibit(Type type)
		{
			return Enumerable.Any<Exhibit>(this._exhibits, (Exhibit exhibit) => exhibit.GetType() == type);
		}

		// Token: 0x06000674 RID: 1652 RVA: 0x00013E19 File Offset: 0x00012019
		public bool HasExhibit<T>() where T : Exhibit
		{
			return this.HasExhibit(typeof(T));
		}

		// Token: 0x06000675 RID: 1653 RVA: 0x00013E2B File Offset: 0x0001202B
		public bool HasExhibit(Exhibit exhibit)
		{
			return this._exhibits.Contains(exhibit);
		}

		// Token: 0x06000676 RID: 1654 RVA: 0x00013E39 File Offset: 0x00012039
		internal void UnsafeAddExhibit(Exhibit exhibit)
		{
			exhibit.Owner = this;
			exhibit.TriggerAdding(this);
			this._exhibits.Add(exhibit);
			exhibit.TriggerAdded(this);
		}

		// Token: 0x06000677 RID: 1655 RVA: 0x00013E5C File Offset: 0x0001205C
		internal void AddExhibit(Exhibit exhibit)
		{
			if (this.HasExhibit(exhibit.GetType()))
			{
				throw new ArgumentException("Cannot add duplicated Exhibit.", "exhibit");
			}
			this.UnsafeAddExhibit(exhibit);
		}

		// Token: 0x06000678 RID: 1656 RVA: 0x00013E83 File Offset: 0x00012083
		internal bool TryAddExhibit(Exhibit exhibit)
		{
			if (this.HasExhibit(exhibit.GetType()))
			{
				return false;
			}
			this.UnsafeAddExhibit(exhibit);
			return true;
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x00013E9D File Offset: 0x0001209D
		internal void InternalRemoveExhibit(Exhibit exhibit)
		{
			exhibit.TriggerRemoving(this);
			this._exhibits.Remove(exhibit);
			exhibit.TriggerRemoved(this);
			exhibit.Owner = null;
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x00013EC1 File Offset: 0x000120C1
		internal void RemoveExhibit(Exhibit exhibit)
		{
			if (!this._exhibits.Contains(exhibit))
			{
				throw new ArgumentException("Exhibit is not owned by the unit.", "exhibit");
			}
			this.InternalRemoveExhibit(exhibit);
		}

		// Token: 0x0600067B RID: 1659 RVA: 0x00013EE8 File Offset: 0x000120E8
		internal void RemoveExhibit<T>() where T : Exhibit
		{
			if (!this.HasExhibit<T>())
			{
				throw new ArgumentException("Exhibit type is not owned by the unit.", "T");
			}
			this.InternalRemoveExhibit(this.GetExhibit<T>());
		}

		// Token: 0x0600067C RID: 1660 RVA: 0x00013F13 File Offset: 0x00012113
		internal bool TryRemoveExhibit(Exhibit exhibit)
		{
			if (!this._exhibits.Contains(exhibit))
			{
				return false;
			}
			this.InternalRemoveExhibit(exhibit);
			return true;
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x00013F2D File Offset: 0x0001212D
		internal bool TryRemoveExhibit<T>() where T : Exhibit
		{
			if (!this.HasExhibit<T>())
			{
				return false;
			}
			this.InternalRemoveExhibit(this.GetExhibit<T>());
			return true;
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x00013F4B File Offset: 0x0001214B
		internal void ClearExhibits()
		{
			this._exhibits.Clear();
		}

		// Token: 0x0600067F RID: 1663 RVA: 0x00013F58 File Offset: 0x00012158
		public override void SetView(IUnitView view)
		{
			base.SetView(view);
			IPlayerUnitView playerUnitView = view as IPlayerUnitView;
			if (playerUnitView != null)
			{
				this.View = playerUnitView;
			}
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x06000680 RID: 1664 RVA: 0x00013F7D File Offset: 0x0001217D
		// (set) Token: 0x06000681 RID: 1665 RVA: 0x00013F85 File Offset: 0x00012185
		[UsedImplicitly]
		public new IPlayerUnitView View { get; private set; }

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x06000682 RID: 1666 RVA: 0x00013F8E File Offset: 0x0001218E
		public GameEvent<PowerEventArgs> PowerGained { get; } = new GameEvent<PowerEventArgs>();

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000683 RID: 1667 RVA: 0x00013F96 File Offset: 0x00012196
		public GameEvent<PowerEventArgs> PowerConsumed { get; } = new GameEvent<PowerEventArgs>();

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000684 RID: 1668 RVA: 0x00013F9E File Offset: 0x0001219E
		public GameEvent<PowerEventArgs> PowerLost { get; } = new GameEvent<PowerEventArgs>();

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000685 RID: 1669 RVA: 0x00013FA6 File Offset: 0x000121A6
		public virtual bool ShowDollSlotByDefault
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000686 RID: 1670 RVA: 0x00013FA9 File Offset: 0x000121A9
		public IReadOnlyList<Doll> Dolls
		{
			get
			{
				return this._dolls.AsReadOnly();
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000687 RID: 1671 RVA: 0x00013FB6 File Offset: 0x000121B6
		public int DollSlotCount
		{
			get
			{
				return this._dollSlotCount;
			}
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x00013FBE File Offset: 0x000121BE
		internal bool HasDoll(Doll doll)
		{
			return this._dolls.Contains(doll);
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x00013FCC File Offset: 0x000121CC
		internal void AddDoll(Doll doll)
		{
			if (this._dolls.Count >= this._dollSlotCount)
			{
				throw new ArgumentException("[] Cannot add " + doll.DebugName + ": slot is full");
			}
			doll.Owner = this;
			doll.TriggerAdding(this);
			this._dolls.Add(doll);
			doll.TriggerAdded(this);
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x00014028 File Offset: 0x00012228
		internal void RemoveDoll(Doll doll)
		{
			if (!this._dolls.Contains(doll))
			{
				throw new ArgumentException(string.Concat(new string[] { "[] Cannot remove ", doll.DebugName, ": ", this.DebugName, " doesn't have this doll" }));
			}
			doll.TriggerRemoving(this);
			this._dolls.Remove(doll);
			doll.TriggerRemoved(this);
			doll.Owner = null;
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x000140A0 File Offset: 0x000122A0
		internal void SetDollSlot(int count)
		{
			if (count < 0 || count > 8)
			{
				throw new ArgumentException(string.Format("Cannot set slot count to {0}", count));
			}
			while (this._dolls.Count > count)
			{
				this.RemoveDoll(this._dolls[count]);
			}
			this._dollSlotCount = count;
		}

		// Token: 0x04000307 RID: 775
		private readonly List<Exhibit> _exhibits = new List<Exhibit>();

		// Token: 0x0400030C RID: 780
		private readonly List<Doll> _dolls = new List<Doll>();

		// Token: 0x0400030D RID: 781
		private int _dollSlotCount;
	}
}
