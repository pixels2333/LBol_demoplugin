using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using UnityEngine;
using UnityEngine.Scripting;

namespace LBoL.Core
{
	// Token: 0x02000014 RID: 20
	[RequireDerived]
	public abstract class GameEntity : IInitializable, INotifyChanged
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x0000352B File Offset: 0x0000172B
		// (set) Token: 0x060000AA RID: 170 RVA: 0x00003533 File Offset: 0x00001733
		public string Id { get; private set; }

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060000AB RID: 171 RVA: 0x0000353C File Offset: 0x0000173C
		public virtual string DebugName
		{
			get
			{
				return string.Concat(new string[] { "<", this.Id, ">(", this.Name, ")" });
			}
		}

		// Token: 0x060000AC RID: 172
		protected abstract string LocalizeProperty(string key, bool decorated = false, bool required = true);

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060000AD RID: 173 RVA: 0x00003573 File Offset: 0x00001773
		protected string BaseName
		{
			get
			{
				return this.LocalizeProperty("Name", false, true);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060000AE RID: 174 RVA: 0x00003582 File Offset: 0x00001782
		protected string BaseDescription
		{
			get
			{
				return this.LocalizeProperty("Description", true, true);
			}
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00003591 File Offset: 0x00001791
		protected virtual string GetBaseDescription()
		{
			return this.BaseDescription;
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x00003599 File Offset: 0x00001799
		public virtual string Name
		{
			get
			{
				return this.BaseName;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x000035A1 File Offset: 0x000017A1
		[UsedImplicitly]
		public string SelfName
		{
			get
			{
				return StringDecorator.GetEntityName(this.Name);
			}
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x000035AE File Offset: 0x000017AE
		[UsedImplicitly]
		public virtual UnitName PlayerName
		{
			get
			{
				GameRunController gameRun = this.GameRun;
				return ((gameRun != null) ? gameRun.Player.GetName() : null) ?? UnitNameTable.GetDefaultPlayerName();
			}
		}

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000B3 RID: 179 RVA: 0x000035D0 File Offset: 0x000017D0
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

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000B4 RID: 180
		internal abstract GameEventPriority DefaultEventPriority { get; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000B5 RID: 181 RVA: 0x00003638 File Offset: 0x00001838
		// (set) Token: 0x060000B6 RID: 182 RVA: 0x00003640 File Offset: 0x00001840
		internal GameEntityFormatWrapper FormatWrapper { get; private set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000B7 RID: 183 RVA: 0x00003649 File Offset: 0x00001849
		// (set) Token: 0x060000B8 RID: 184 RVA: 0x00003651 File Offset: 0x00001851
		public GameRunController GameRun { get; internal set; }

		// Token: 0x060000B9 RID: 185 RVA: 0x0000365A File Offset: 0x0000185A
		protected virtual void React(Reactor reactor)
		{
			throw new NotSupportedException("Cannot react from " + base.GetType().Name);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00003676 File Offset: 0x00001876
		protected void React(IEnumerable<BattleAction> sequencedReactor)
		{
			this.React(new Reactor(sequencedReactor));
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00003684 File Offset: 0x00001884
		protected void React(LazyActionReactor lazyActionReactor)
		{
			this.React(new Reactor(lazyActionReactor));
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00003692 File Offset: 0x00001892
		protected void React(LazySequencedReactor lazySequencedReactor)
		{
			this.React(new Reactor(lazySequencedReactor));
		}

		// Token: 0x060000BD RID: 189 RVA: 0x000036A0 File Offset: 0x000018A0
		public virtual void Initialize()
		{
			this.Id = base.GetType().Name;
			this.FormatWrapper = this.CreateFormatWrapper();
		}

		// Token: 0x060000BE RID: 190 RVA: 0x000036BF File Offset: 0x000018BF
		internal virtual GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new GameEntityFormatWrapper(this);
		}

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x060000BF RID: 191 RVA: 0x000036C8 File Offset: 0x000018C8
		// (remove) Token: 0x060000C0 RID: 192 RVA: 0x00003700 File Offset: 0x00001900
		public event Action PropertyChanged;

		// Token: 0x060000C1 RID: 193 RVA: 0x00003735 File Offset: 0x00001935
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
