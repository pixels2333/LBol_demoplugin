using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using UnityEngine;
using UnityEngine.Scripting;
namespace LBoL.Core
{
	[RequireDerived]
	public abstract class GameEntity : IInitializable, INotifyChanged
	{
		public string Id { get; private set; }
		public virtual string DebugName
		{
			get
			{
				return string.Concat(new string[] { "<", this.Id, ">(", this.Name, ")" });
			}
		}
		protected abstract string LocalizeProperty(string key, bool decorated = false, bool required = true);
		protected string BaseName
		{
			get
			{
				return this.LocalizeProperty("Name", false, true);
			}
		}
		protected string BaseDescription
		{
			get
			{
				return this.LocalizeProperty("Description", true, true);
			}
		}
		protected virtual string GetBaseDescription()
		{
			return this.BaseDescription;
		}
		public virtual string Name
		{
			get
			{
				return this.BaseName;
			}
		}
		[UsedImplicitly]
		public string SelfName
		{
			get
			{
				return StringDecorator.GetEntityName(this.Name);
			}
		}
		[UsedImplicitly]
		public virtual UnitName PlayerName
		{
			get
			{
				GameRunController gameRun = this.GameRun;
				return ((gameRun != null) ? gameRun.Player.GetName() : null) ?? UnitNameTable.GetDefaultPlayerName();
			}
		}
		public virtual string Description
		{
			get
			{
				string text;
				try
				{
					string baseDescription = this.GetBaseDescription();
					if (baseDescription == null)
					{
						Debug.LogError("<" + this.DebugName + ">.GetBaseDescription() returns null");
						text = "";
					}
					else
					{
						text = baseDescription.RuntimeFormat(this.FormatWrapper);
					}
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					text = "<Error>";
				}
				return text;
			}
		}
		internal abstract GameEventPriority DefaultEventPriority { get; }
		internal GameEntityFormatWrapper FormatWrapper { get; private set; }
		public GameRunController GameRun { get; internal set; }
		protected virtual void React(Reactor reactor)
		{
			throw new NotSupportedException("Cannot react from " + base.GetType().Name);
		}
		protected void React(IEnumerable<BattleAction> sequencedReactor)
		{
			this.React(new Reactor(sequencedReactor));
		}
		protected void React(LazyActionReactor lazyActionReactor)
		{
			this.React(new Reactor(lazyActionReactor));
		}
		protected void React(LazySequencedReactor lazySequencedReactor)
		{
			this.React(new Reactor(lazySequencedReactor));
		}
		public virtual void Initialize()
		{
			this.Id = base.GetType().Name;
			this.FormatWrapper = this.CreateFormatWrapper();
		}
		internal virtual GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new GameEntityFormatWrapper(this);
		}
		public event Action PropertyChanged;
		public virtual void NotifyChanged()
		{
			Action propertyChanged = this.PropertyChanged;
			if (propertyChanged == null)
			{
				return;
			}
			propertyChanged.Invoke();
		}
	}
}
