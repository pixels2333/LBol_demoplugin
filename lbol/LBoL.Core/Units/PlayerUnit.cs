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
	[Localizable]
	public abstract class PlayerUnit : Unit
	{
		public bool IsSuperExtraTurn
		{
			get
			{
				return base.IsExtraTurn && base.HasStatusEffect<SuperExtraTurn>() && base.GetStatusEffect<SuperExtraTurn>().IsInExtraTurnByThis;
			}
		}
		public PlayerUnitConfig Config { get; private set; }
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
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<PlayerUnit>.LocalizeProperty(base.Id, key, decorated, required);
		}
		public string Title
		{
			get
			{
				return this.LocalizeProperty("Title", false, true);
			}
		}
		public override UnitName GetName()
		{
			return UnitNameTable.GetName(base.Id, this.Config.NarrativeColor);
		}
		public bool HasHomeName
		{
			get
			{
				return this.Config.HasHomeName;
			}
		}
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
		public string HomeName
		{
			get
			{
				return this.LocalizeProperty("HomeName", false, true);
			}
		}
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
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}
		public bool HasUs
		{
			get
			{
				return this.Us != null;
			}
		}
		public UltimateSkill Us { get; private set; }
		public int Power { get; internal set; }
		public int PowerLevel
		{
			get
			{
				return this.Power / this.PowerPerLevel;
			}
		}
		public int PowerRemainder
		{
			get
			{
				return this.Power % this.PowerPerLevel;
			}
		}
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
		public int MaxPower
		{
			get
			{
				return this.MaxPowerLevel * this.PowerPerLevel;
			}
		}
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
		public void SetUs(UltimateSkill us)
		{
			if (this.Us != null)
			{
				throw new InvalidOperationException(this.Name + " already has US " + this.Us.Name);
			}
			this.Us = us;
			us.Owner = this;
		}
		public IReadOnlyList<Exhibit> Exhibits
		{
			get
			{
				return this._exhibits.AsReadOnly();
			}
		}
		public Exhibit GetExhibit(string id)
		{
			return Enumerable.FirstOrDefault<Exhibit>(this._exhibits, (Exhibit exhibit) => exhibit.Id == id);
		}
		public Exhibit GetExhibit(Type type)
		{
			return Enumerable.FirstOrDefault<Exhibit>(this._exhibits, (Exhibit exhibit) => exhibit.GetType() == type);
		}
		public T GetExhibit<T>() where T : Exhibit
		{
			return (T)((object)this.GetExhibit(typeof(T)));
		}
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
		public bool HasExhibit(Type type)
		{
			return Enumerable.Any<Exhibit>(this._exhibits, (Exhibit exhibit) => exhibit.GetType() == type);
		}
		public bool HasExhibit<T>() where T : Exhibit
		{
			return this.HasExhibit(typeof(T));
		}
		public bool HasExhibit(Exhibit exhibit)
		{
			return this._exhibits.Contains(exhibit);
		}
		internal void UnsafeAddExhibit(Exhibit exhibit)
		{
			exhibit.Owner = this;
			exhibit.TriggerAdding(this);
			this._exhibits.Add(exhibit);
			exhibit.TriggerAdded(this);
		}
		internal void AddExhibit(Exhibit exhibit)
		{
			if (this.HasExhibit(exhibit.GetType()))
			{
				throw new ArgumentException("Cannot add duplicated Exhibit.", "exhibit");
			}
			this.UnsafeAddExhibit(exhibit);
		}
		internal bool TryAddExhibit(Exhibit exhibit)
		{
			if (this.HasExhibit(exhibit.GetType()))
			{
				return false;
			}
			this.UnsafeAddExhibit(exhibit);
			return true;
		}
		internal void InternalRemoveExhibit(Exhibit exhibit)
		{
			exhibit.TriggerRemoving(this);
			this._exhibits.Remove(exhibit);
			exhibit.TriggerRemoved(this);
			exhibit.Owner = null;
		}
		internal void RemoveExhibit(Exhibit exhibit)
		{
			if (!this._exhibits.Contains(exhibit))
			{
				throw new ArgumentException("Exhibit is not owned by the unit.", "exhibit");
			}
			this.InternalRemoveExhibit(exhibit);
		}
		internal void RemoveExhibit<T>() where T : Exhibit
		{
			if (!this.HasExhibit<T>())
			{
				throw new ArgumentException("Exhibit type is not owned by the unit.", "T");
			}
			this.InternalRemoveExhibit(this.GetExhibit<T>());
		}
		internal bool TryRemoveExhibit(Exhibit exhibit)
		{
			if (!this._exhibits.Contains(exhibit))
			{
				return false;
			}
			this.InternalRemoveExhibit(exhibit);
			return true;
		}
		internal bool TryRemoveExhibit<T>() where T : Exhibit
		{
			if (!this.HasExhibit<T>())
			{
				return false;
			}
			this.InternalRemoveExhibit(this.GetExhibit<T>());
			return true;
		}
		internal void ClearExhibits()
		{
			this._exhibits.Clear();
		}
		public override void SetView(IUnitView view)
		{
			base.SetView(view);
			IPlayerUnitView playerUnitView = view as IPlayerUnitView;
			if (playerUnitView != null)
			{
				this.View = playerUnitView;
			}
		}
		[UsedImplicitly]
		public new IPlayerUnitView View { get; private set; }
		public GameEvent<PowerEventArgs> PowerGained { get; } = new GameEvent<PowerEventArgs>();
		public GameEvent<PowerEventArgs> PowerConsumed { get; } = new GameEvent<PowerEventArgs>();
		public GameEvent<PowerEventArgs> PowerLost { get; } = new GameEvent<PowerEventArgs>();
		public virtual bool ShowDollSlotByDefault
		{
			get
			{
				return false;
			}
		}
		public IReadOnlyList<Doll> Dolls
		{
			get
			{
				return this._dolls.AsReadOnly();
			}
		}
		public int DollSlotCount
		{
			get
			{
				return this._dollSlotCount;
			}
		}
		internal bool HasDoll(Doll doll)
		{
			return this._dolls.Contains(doll);
		}
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
		private readonly List<Exhibit> _exhibits = new List<Exhibit>();
		private readonly List<Doll> _dolls = new List<Doll>();
		private int _dollSlotCount;
	}
}
