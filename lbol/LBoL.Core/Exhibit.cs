using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x0200000C RID: 12
	[Localizable]
	public abstract class Exhibit : GameEntity, INotifyActivating
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600003E RID: 62 RVA: 0x000029D1 File Offset: 0x00000BD1
		// (set) Token: 0x0600003F RID: 63 RVA: 0x000029D9 File Offset: 0x00000BD9
		public ExhibitConfig Config { get; private set; }

		// Token: 0x06000040 RID: 64 RVA: 0x000029E2 File Offset: 0x00000BE2
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Exhibit>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000029F2 File Offset: 0x00000BF2
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<Exhibit>.LocalizeListProperty(base.Id, key, required);
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00002A01 File Offset: 0x00000C01
		protected override string GetBaseDescription()
		{
			return this.LocalizeProperty("Description", true, false);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00002A10 File Offset: 0x00000C10
		public EntityName GetName()
		{
			return new EntityName("Exhibit." + base.Id, this.Name);
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000044 RID: 68 RVA: 0x00002A30 File Offset: 0x00000C30
		public override string Description
		{
			get
			{
				string text;
				try
				{
					string baseDescription = this.GetBaseDescription();
					text = ((baseDescription == null) ? "" : baseDescription.RuntimeFormat(base.FormatWrapper));
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					text = "<Error>";
				}
				return text;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000045 RID: 69 RVA: 0x00002A7C File Offset: 0x00000C7C
		public string FlavorText
		{
			get
			{
				return this.LocalizeProperty("FlavorText", false, false);
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00002A8B File Offset: 0x00000C8B
		protected string ExtraDescription
		{
			get
			{
				return this.LocalizeProperty("ExtraDescription", true, true);
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000047 RID: 71 RVA: 0x00002A9A File Offset: 0x00000C9A
		// (set) Token: 0x06000048 RID: 72 RVA: 0x00002AA2 File Offset: 0x00000CA2
		public string OwnerId { get; private set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000049 RID: 73 RVA: 0x00002AAB File Offset: 0x00000CAB
		public virtual string IconName
		{
			get
			{
				return base.Id;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00002AB3 File Offset: 0x00000CB3
		public virtual string OverrideIconName
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00002AB6 File Offset: 0x00000CB6
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00002AC4 File Offset: 0x00000CC4
		public override void Initialize()
		{
			base.Initialize();
			this.Config = ExhibitConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find exhibit config for <" + base.Id + ">");
			}
			this.OwnerId = (this.Config.Owner.IsNullOrWhiteSpace() ? null : this.Config.Owner);
			if (this.Config.HasCounter)
			{
				int? initialCounter = this.Config.InitialCounter;
				if (initialCounter != null)
				{
					int valueOrDefault = initialCounter.GetValueOrDefault();
					this.Counter = valueOrDefault;
				}
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600004D RID: 77 RVA: 0x00002B64 File Offset: 0x00000D64
		// (set) Token: 0x0600004E RID: 78 RVA: 0x00002B83 File Offset: 0x00000D83
		public PlayerUnit Owner
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
			internal set
			{
				this._owner.SetTarget(value);
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600004F RID: 79 RVA: 0x00002B94 File Offset: 0x00000D94
		public int Value1
		{
			get
			{
				int? value = this.Config.Value1;
				if (value == null)
				{
					throw new InvalidDataException(this.DebugName + " has no 'Value1' in config");
				}
				return value.GetValueOrDefault();
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000050 RID: 80 RVA: 0x00002BD4 File Offset: 0x00000DD4
		public int Value2
		{
			get
			{
				int? value = this.Config.Value2;
				if (value == null)
				{
					throw new InvalidDataException(this.DebugName + " has no 'Value2' in config");
				}
				return value.GetValueOrDefault();
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000051 RID: 81 RVA: 0x00002C14 File Offset: 0x00000E14
		public int Value3
		{
			get
			{
				int? value = this.Config.Value3;
				if (value == null)
				{
					throw new InvalidDataException(this.DebugName + " has no 'Value3' in config");
				}
				return value.GetValueOrDefault();
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000052 RID: 82 RVA: 0x00002C54 File Offset: 0x00000E54
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				ManaGroup? mana = this.Config.Mana;
				if (mana == null)
				{
					throw new InvalidDataException("<" + base.Id + "> has empty Mana config");
				}
				return mana.GetValueOrDefault();
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000053 RID: 83 RVA: 0x00002C98 File Offset: 0x00000E98
		[UsedImplicitly]
		public BaseManaGroup BaseMana
		{
			get
			{
				ManaColor? baseManaColor = this.Config.BaseManaColor;
				if (baseManaColor != null)
				{
					ManaColor valueOrDefault = baseManaColor.GetValueOrDefault();
					return new BaseManaGroup(ManaGroup.Single(valueOrDefault) * this.Config.BaseManaAmount);
				}
				return new BaseManaGroup(ManaGroup.Empty);
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000054 RID: 84 RVA: 0x00002CEA File Offset: 0x00000EEA
		public bool HasCounter
		{
			get
			{
				return this.Config.HasCounter;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000055 RID: 85 RVA: 0x00002CF8 File Offset: 0x00000EF8
		[UsedImplicitly]
		public int InitialCounter
		{
			get
			{
				return this.Config.InitialCounter.GetValueOrDefault();
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000056 RID: 86 RVA: 0x00002D18 File Offset: 0x00000F18
		public virtual bool ShowCounter
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000057 RID: 87 RVA: 0x00002D1B File Offset: 0x00000F1B
		// (set) Token: 0x06000058 RID: 88 RVA: 0x00002D23 File Offset: 0x00000F23
		public bool Active
		{
			get
			{
				return this._active;
			}
			set
			{
				if (this._active != value)
				{
					this._active = value;
					this.NotifyChanged();
				}
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00002D3B File Offset: 0x00000F3B
		// (set) Token: 0x0600005A RID: 90 RVA: 0x00002D43 File Offset: 0x00000F43
		public bool Blackout
		{
			get
			{
				return this._blackout;
			}
			set
			{
				if (this._blackout != value)
				{
					this._blackout = value;
					this.NotifyChanged();
				}
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00002D5B File Offset: 0x00000F5B
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00002D64 File Offset: 0x00000F64
		public int Counter
		{
			get
			{
				return this._counter;
			}
			set
			{
				int num = Math.Min(value, 999);
				if (this._counter != num)
				{
					this._counter = num;
					this.NotifyChanged();
				}
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00002D93 File Offset: 0x00000F93
		public ExhibitLosableType LosableType
		{
			get
			{
				return this.Config.LosableType;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600005E RID: 94 RVA: 0x00002DA0 File Offset: 0x00000FA0
		public bool IsSentinel
		{
			get
			{
				return this.Config.IsSentinel;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00002DAD File Offset: 0x00000FAD
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00002DB5 File Offset: 0x00000FB5
		public int? CardInstanceId { get; protected internal set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000061 RID: 97 RVA: 0x00002DBE File Offset: 0x00000FBE
		public BattleController Battle
		{
			get
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun == null)
				{
					return null;
				}
				return gameRun.Battle;
			}
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002DD1 File Offset: 0x00000FD1
		internal void TriggerAdding(PlayerUnit unit)
		{
			this.OnAdding(unit);
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002DDA File Offset: 0x00000FDA
		internal void TriggerAdded(PlayerUnit unit)
		{
			this.OnAdded(unit);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00002DE3 File Offset: 0x00000FE3
		internal void TriggerRemoving(PlayerUnit unit)
		{
			this.OnRemoving(unit);
			this._gameRunHandlerHolder.ClearEventHandlers();
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00002DF7 File Offset: 0x00000FF7
		internal void TriggerRemoved(PlayerUnit unit)
		{
			this.OnRemoved(unit);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00002E00 File Offset: 0x00001000
		internal IEnumerator TriggerGain(PlayerUnit unit)
		{
			IEnumerator enumerator = this.SpecialGain(unit);
			if (enumerator != null)
			{
				yield return enumerator;
			}
			else
			{
				this.OnGain(unit);
			}
			yield break;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00002E16 File Offset: 0x00001016
		internal void TriggerGainInstantly(PlayerUnit unit)
		{
			if (this.SpecialGain(unit) != null)
			{
				Debug.LogError("Cannot gain " + this.DebugName + " instantly (init-exhibit or debug-add) because it has special gain interaction, triggering OnGain() instead.");
			}
			this.OnGain(unit);
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002E42 File Offset: 0x00001042
		internal void TriggerLose(PlayerUnit unit)
		{
			if (this.Battle != null)
			{
				this.OnLeaveBattle();
				this._battleHandlerHolder.ClearEventHandlers();
			}
			this.OnLose(unit);
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00002E64 File Offset: 0x00001064
		internal void EnterBattle()
		{
			this.OnEnterBattle();
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00002E6C File Offset: 0x0000106C
		internal void LeaveBattle()
		{
			this.OnLeaveBattle();
			this._battleHandlerHolder.ClearEventHandlers();
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002E7F File Offset: 0x0000107F
		protected virtual void OnAdding(PlayerUnit player)
		{
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00002E81 File Offset: 0x00001081
		protected virtual void OnAdded(PlayerUnit player)
		{
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00002E83 File Offset: 0x00001083
		protected virtual void OnRemoving(PlayerUnit player)
		{
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00002E85 File Offset: 0x00001085
		protected virtual void OnRemoved(PlayerUnit player)
		{
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00002E87 File Offset: 0x00001087
		protected virtual IEnumerator SpecialGain(PlayerUnit unit)
		{
			return null;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00002E8A File Offset: 0x0000108A
		protected virtual void OnGain(PlayerUnit player)
		{
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00002E8C File Offset: 0x0000108C
		protected virtual void OnLose(PlayerUnit player)
		{
		}

		// Token: 0x06000072 RID: 114 RVA: 0x00002E8E File Offset: 0x0000108E
		protected virtual void OnEnterBattle()
		{
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00002E90 File Offset: 0x00001090
		protected virtual void OnLeaveBattle()
		{
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00002E92 File Offset: 0x00001092
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000075 RID: 117 RVA: 0x00002EA4 File Offset: 0x000010A4
		// (remove) Token: 0x06000076 RID: 118 RVA: 0x00002EDC File Offset: 0x000010DC
		public event Action Activating;

		// Token: 0x06000077 RID: 119 RVA: 0x00002F11 File Offset: 0x00001111
		protected override void React(Reactor reactor)
		{
			if (this.Battle == null)
			{
				Debug.LogError("[Exhibit: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.Exhibit);
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00002F44 File Offset: 0x00001144
		public virtual IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Config.Keywords, this.Config.RelativeEffects, verbose, default(Keyword?));
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00002F7C File Offset: 0x0000117C
		public virtual IEnumerable<Card> EnumerateRelativeCards()
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

		// Token: 0x0600007A RID: 122 RVA: 0x00002F8C File Offset: 0x0000118C
		protected void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this._gameRunHandlerHolder.HandleEvent<TEventArgs>(@event, handler, priority);
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00002F9C File Offset: 0x0000119C
		protected void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
		{
			this.HandleGameRunEvent<TEventArgs>(@event, handler, this.DefaultEventPriority);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00002FAC File Offset: 0x000011AC
		protected void HandleBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this._battleHandlerHolder.HandleEvent<TEventArgs>(@event, handler, priority);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00002FBC File Offset: 0x000011BC
		protected void HandleBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, handler, this.DefaultEventPriority);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00002FCC File Offset: 0x000011CC
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003001 File Offset: 0x00001201
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.ReactBattleEvent<TEventArgs>(@event, reactor, this.DefaultEventPriority);
		}

		// Token: 0x0400005E RID: 94
		private readonly WeakReference<PlayerUnit> _owner = new WeakReference<PlayerUnit>(null);

		// Token: 0x0400005F RID: 95
		private bool _active;

		// Token: 0x04000060 RID: 96
		private bool _blackout;

		// Token: 0x04000061 RID: 97
		private int _counter;

		// Token: 0x04000064 RID: 100
		private readonly GameEventHandlerHolder _gameRunHandlerHolder = new GameEventHandlerHolder();

		// Token: 0x04000065 RID: 101
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();
	}
}
