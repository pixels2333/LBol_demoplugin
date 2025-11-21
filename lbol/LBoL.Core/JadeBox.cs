using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Attributes;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000055 RID: 85
	[Localizable]
	public abstract class JadeBox : GameEntity
	{
		// Token: 0x17000136 RID: 310
		// (get) Token: 0x0600037A RID: 890 RVA: 0x0000B7F7 File Offset: 0x000099F7
		// (set) Token: 0x0600037B RID: 891 RVA: 0x0000B7FF File Offset: 0x000099FF
		public JadeBoxConfig Config { get; private set; }

		// Token: 0x0600037C RID: 892 RVA: 0x0000B808 File Offset: 0x00009A08
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<JadeBox>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x0600037D RID: 893 RVA: 0x0000B818 File Offset: 0x00009A18
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<JadeBox>.LocalizeListProperty(base.Id, key, required);
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x0600037E RID: 894 RVA: 0x0000B828 File Offset: 0x00009A28
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

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x0600037F RID: 895 RVA: 0x0000B874 File Offset: 0x00009A74
		public virtual string IconName
		{
			get
			{
				return base.Id;
			}
		}

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x06000380 RID: 896 RVA: 0x0000B87C File Offset: 0x00009A7C
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}

		// Token: 0x06000381 RID: 897 RVA: 0x0000B889 File Offset: 0x00009A89
		public override void Initialize()
		{
			base.Initialize();
			this.Config = JadeBoxConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find JadeBox config for <" + base.Id + ">");
			}
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06000382 RID: 898 RVA: 0x0000B8C8 File Offset: 0x00009AC8
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

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06000383 RID: 899 RVA: 0x0000B908 File Offset: 0x00009B08
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

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06000384 RID: 900 RVA: 0x0000B948 File Offset: 0x00009B48
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

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06000385 RID: 901 RVA: 0x0000B988 File Offset: 0x00009B88
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

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06000386 RID: 902 RVA: 0x0000B9CC File Offset: 0x00009BCC
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

		// Token: 0x06000387 RID: 903 RVA: 0x0000B9DF File Offset: 0x00009BDF
		internal void TriggerGain(GameRunController gameRun)
		{
			this.OnGain(gameRun);
		}

		// Token: 0x06000388 RID: 904 RVA: 0x0000B9E8 File Offset: 0x00009BE8
		internal void TriggerAdded()
		{
			this.OnAdded();
		}

		// Token: 0x06000389 RID: 905 RVA: 0x0000B9F0 File Offset: 0x00009BF0
		internal void EnterBattle()
		{
			this.OnEnterBattle();
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0000B9F8 File Offset: 0x00009BF8
		internal void LeaveBattle()
		{
			this.OnLeaveBattle();
			this._battleHandlerHolder.ClearEventHandlers();
		}

		// Token: 0x0600038B RID: 907 RVA: 0x0000BA0B File Offset: 0x00009C0B
		protected virtual void OnGain(GameRunController gameRun)
		{
		}

		// Token: 0x0600038C RID: 908 RVA: 0x0000BA0D File Offset: 0x00009C0D
		protected virtual void OnAdded()
		{
		}

		// Token: 0x0600038D RID: 909 RVA: 0x0000BA0F File Offset: 0x00009C0F
		protected virtual void OnEnterBattle()
		{
		}

		// Token: 0x0600038E RID: 910 RVA: 0x0000BA11 File Offset: 0x00009C11
		protected virtual void OnLeaveBattle()
		{
		}

		// Token: 0x0600038F RID: 911 RVA: 0x0000BA13 File Offset: 0x00009C13
		public void NotifyActivating()
		{
			Action activating = this.Activating;
			if (activating == null)
			{
				return;
			}
			activating.Invoke();
		}

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000390 RID: 912 RVA: 0x0000BA28 File Offset: 0x00009C28
		// (remove) Token: 0x06000391 RID: 913 RVA: 0x0000BA60 File Offset: 0x00009C60
		public event Action Activating;

		// Token: 0x06000392 RID: 914 RVA: 0x0000BA95 File Offset: 0x00009C95
		protected override void React(Reactor reactor)
		{
			if (this.Battle == null)
			{
				Debug.LogError("[JadeBox: " + this.DebugName + "] Cannot react outside battle");
				return;
			}
			this.Battle.React(reactor, this, ActionCause.JadeBox);
		}

		// Token: 0x06000393 RID: 915 RVA: 0x0000BACC File Offset: 0x00009CCC
		public virtual IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Config.Keywords, this.Config.RelativeEffects, verbose, default(Keyword?));
		}

		// Token: 0x06000394 RID: 916 RVA: 0x0000BB04 File Offset: 0x00009D04
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

		// Token: 0x06000395 RID: 917 RVA: 0x0000BB14 File Offset: 0x00009D14
		protected void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this._gameRunHandlerHolder.HandleEvent<TEventArgs>(@event, handler, priority);
		}

		// Token: 0x06000396 RID: 918 RVA: 0x0000BB24 File Offset: 0x00009D24
		protected void HandleGameRunEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
		{
			this.HandleGameRunEvent<TEventArgs>(@event, handler, this.DefaultEventPriority);
		}

		// Token: 0x06000397 RID: 919 RVA: 0x0000BB34 File Offset: 0x00009D34
		protected void HandleBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this._battleHandlerHolder.HandleEvent<TEventArgs>(@event, handler, priority);
		}

		// Token: 0x06000398 RID: 920 RVA: 0x0000BB44 File Offset: 0x00009D44
		protected void HandleBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, GameEventHandler<TEventArgs> handler) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, handler, this.DefaultEventPriority);
		}

		// Token: 0x06000399 RID: 921 RVA: 0x0000BB54 File Offset: 0x00009D54
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleBattleEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}

		// Token: 0x0600039A RID: 922 RVA: 0x0000BB89 File Offset: 0x00009D89
		protected void ReactBattleEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.ReactBattleEvent<TEventArgs>(@event, reactor, this.DefaultEventPriority);
		}

		// Token: 0x0400020D RID: 525
		private readonly GameEventHandlerHolder _gameRunHandlerHolder = new GameEventHandlerHolder();

		// Token: 0x0400020E RID: 526
		private readonly GameEventHandlerHolder _battleHandlerHolder = new GameEventHandlerHolder();
	}
}
