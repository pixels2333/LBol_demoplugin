using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using UnityEngine;

namespace LBoL.Core.Units
{
	// Token: 0x0200007B RID: 123
	public abstract class Doll : GameEntity
	{
		// Token: 0x0600056D RID: 1389 RVA: 0x0001201C File Offset: 0x0001021C
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<Doll>.LocalizeProperty(base.Id, key, decorated, required);
		}

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x0600056E RID: 1390 RVA: 0x0001202C File Offset: 0x0001022C
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return GameEventPriority.ConfigDefault;
			}
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x0600056F RID: 1391 RVA: 0x00012030 File Offset: 0x00010230
		// (set) Token: 0x06000570 RID: 1392 RVA: 0x00012038 File Offset: 0x00010238
		public DollConfig Config { get; protected set; }

		// Token: 0x06000571 RID: 1393 RVA: 0x00012044 File Offset: 0x00010244
		public override void Initialize()
		{
			base.Initialize();
			this.Config = DollConfig.FromId(base.Id);
			if (this.Config == null)
			{
				throw new InvalidDataException("Cannot find doll config for <" + base.Id + ">");
			}
			this.Magic = this.InitialMagic;
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x00012098 File Offset: 0x00010298
		public IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Config.Keywords, this.Config.RelativeEffects, verbose, default(Keyword?));
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x000120D0 File Offset: 0x000102D0
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new Doll.DollFormatWrapper(this);
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000574 RID: 1396 RVA: 0x000120D8 File Offset: 0x000102D8
		[UsedImplicitly]
		public virtual DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)this.ConfigDamage, false);
			}
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000575 RID: 1397 RVA: 0x000120E8 File Offset: 0x000102E8
		public int ConfigDamage
		{
			get
			{
				int? damage = this.Config.Damage;
				if (damage != null)
				{
					return damage.GetValueOrDefault();
				}
				throw new InvalidDataException("<" + this.DebugName + "> has empty damage config");
			}
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000576 RID: 1398 RVA: 0x00012130 File Offset: 0x00010330
		public int Value1
		{
			get
			{
				int? value = this.Config.Value1;
				if (value != null)
				{
					int valueOrDefault = value.GetValueOrDefault();
					return this.CalculateDollValue(valueOrDefault);
				}
				throw new InvalidDataException(this.DebugName + " has no 'Value1' in config");
			}
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000577 RID: 1399 RVA: 0x0001217C File Offset: 0x0001037C
		public int Value2
		{
			get
			{
				int? value = this.Config.Value2;
				if (value != null)
				{
					int valueOrDefault = value.GetValueOrDefault();
					return this.CalculateDollValue(valueOrDefault);
				}
				throw new InvalidDataException(this.DebugName + " has no 'Value2' in config");
			}
		}

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000578 RID: 1400 RVA: 0x000121C5 File Offset: 0x000103C5
		// (set) Token: 0x06000579 RID: 1401 RVA: 0x000121CD File Offset: 0x000103CD
		public int Magic
		{
			get
			{
				return this._magic;
			}
			set
			{
				this._magic = value;
				this.NotifyChanged();
			}
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x0600057A RID: 1402 RVA: 0x000121DC File Offset: 0x000103DC
		public bool Usable
		{
			get
			{
				return this.Config.Usable;
			}
		}

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x0600057B RID: 1403 RVA: 0x000121E9 File Offset: 0x000103E9
		public bool HasMagic
		{
			get
			{
				return this.Config.HasMagic;
			}
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x0600057C RID: 1404 RVA: 0x000121F6 File Offset: 0x000103F6
		public int InitialMagic
		{
			get
			{
				return this.Config.InitialMagic;
			}
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x0600057D RID: 1405 RVA: 0x00012203 File Offset: 0x00010403
		public int MagicCost
		{
			get
			{
				return this.Config.MagicCost;
			}
		}

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x0600057E RID: 1406 RVA: 0x00012210 File Offset: 0x00010410
		public int MaxMagic
		{
			get
			{
				return this.Config.MaxMagic;
			}
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x0600057F RID: 1407 RVA: 0x00012220 File Offset: 0x00010420
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

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000580 RID: 1408 RVA: 0x00012264 File Offset: 0x00010464
		public virtual int? UpCounter
		{
			get
			{
				return default(int?);
			}
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000581 RID: 1409 RVA: 0x0001227A File Offset: 0x0001047A
		public virtual Color UpCounterColor
		{
			get
			{
				return Color.white;
			}
		}

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000582 RID: 1410 RVA: 0x00012284 File Offset: 0x00010484
		public virtual int? DownCounter
		{
			get
			{
				return default(int?);
			}
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06000583 RID: 1411 RVA: 0x0001229A File Offset: 0x0001049A
		public virtual Color DownCounterColor
		{
			get
			{
				return Color.white;
			}
		}

		// Token: 0x06000584 RID: 1412 RVA: 0x000122A1 File Offset: 0x000104A1
		protected int CalculateDamage(DamageInfo damage)
		{
			if (this.Owner == null)
			{
				return damage.Damage.RoundToInt(1);
			}
			return this.Owner.Battle.CalculateDamage(this, this.Owner, null, damage);
		}

		// Token: 0x06000585 RID: 1413 RVA: 0x000122D2 File Offset: 0x000104D2
		protected int CalculateBlock(BlockInfo block)
		{
			if (this.Owner == null)
			{
				return block.Block;
			}
			return this.Owner.Battle.CalculateBlockShield(this, (float)block.Block, 0f, block.Type).Item1;
		}

		// Token: 0x06000586 RID: 1414 RVA: 0x00012310 File Offset: 0x00010510
		protected int CalculateDollValue(int value)
		{
			PlayerUnit owner = this.Owner;
			int? num;
			if (owner == null)
			{
				num = default(int?);
			}
			else
			{
				BattleController battle = owner.Battle;
				num = ((battle != null) ? new int?(battle.CalculateDollValue(this, value)) : default(int?));
			}
			int? num2 = num;
			if (num2 == null)
			{
				return value;
			}
			return num2.GetValueOrDefault();
		}

		// Token: 0x06000587 RID: 1415 RVA: 0x00012365 File Offset: 0x00010565
		protected void HandleOwnerEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler, GameEventPriority priority) where T : GameEventArgs
		{
			this._ownerHandlerHolder.HandleEvent<T>(@event, handler, priority);
		}

		// Token: 0x06000588 RID: 1416 RVA: 0x00012375 File Offset: 0x00010575
		protected void HandleOwnerEvent<T>(GameEvent<T> @event, GameEventHandler<T> handler) where T : GameEventArgs
		{
			this.HandleOwnerEvent<T>(@event, handler, this.DefaultEventPriority);
		}

		// Token: 0x06000589 RID: 1417 RVA: 0x00012388 File Offset: 0x00010588
		protected void ReactOwnerEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor, GameEventPriority priority) where TEventArgs : GameEventArgs
		{
			this.HandleOwnerEvent<TEventArgs>(@event, delegate(TEventArgs args)
			{
				this.React(reactor(args));
			}, priority);
		}

		// Token: 0x0600058A RID: 1418 RVA: 0x000123BD File Offset: 0x000105BD
		protected void ReactOwnerEvent<TEventArgs>(GameEvent<TEventArgs> @event, EventSequencedReactor<TEventArgs> reactor) where TEventArgs : GameEventArgs
		{
			this.ReactOwnerEvent<TEventArgs>(@event, reactor, this.DefaultEventPriority);
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x0600058B RID: 1419 RVA: 0x000123D0 File Offset: 0x000105D0
		// (set) Token: 0x0600058C RID: 1420 RVA: 0x000123EF File Offset: 0x000105EF
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

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x0600058D RID: 1421 RVA: 0x000123FD File Offset: 0x000105FD
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

		// Token: 0x0600058E RID: 1422 RVA: 0x00012410 File Offset: 0x00010610
		internal void TriggerAdding(PlayerUnit owner)
		{
			this.OnAdding(owner);
		}

		// Token: 0x0600058F RID: 1423 RVA: 0x00012419 File Offset: 0x00010619
		internal void TriggerAdded(PlayerUnit owner)
		{
			this.OnAdded(owner);
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x00012422 File Offset: 0x00010622
		internal void TriggerRemoving(PlayerUnit owner)
		{
			this.OnRemoving(owner);
		}

		// Token: 0x06000591 RID: 1425 RVA: 0x0001242B File Offset: 0x0001062B
		internal void TriggerRemoved(PlayerUnit owner)
		{
			this.OnRemoved(owner);
			this._ownerHandlerHolder.ClearEventHandlers();
		}

		// Token: 0x06000592 RID: 1426 RVA: 0x0001243F File Offset: 0x0001063F
		protected virtual void OnAdding(PlayerUnit owner)
		{
		}

		// Token: 0x06000593 RID: 1427 RVA: 0x00012441 File Offset: 0x00010641
		protected virtual void OnAdded(PlayerUnit owner)
		{
		}

		// Token: 0x06000594 RID: 1428 RVA: 0x00012443 File Offset: 0x00010643
		protected virtual void OnRemoving(PlayerUnit owner)
		{
		}

		// Token: 0x06000595 RID: 1429 RVA: 0x00012445 File Offset: 0x00010645
		protected virtual void OnRemoved(PlayerUnit owner)
		{
		}

		// Token: 0x06000596 RID: 1430 RVA: 0x00012447 File Offset: 0x00010647
		internal IEnumerable<BattleAction> GetPassiveActions(List<DamageAction> damageActions)
		{
			if (this.PassiveActions() == null)
			{
				yield break;
			}
			foreach (BattleAction battleAction in this.PassiveActions())
			{
				DamageAction damageAction = battleAction as DamageAction;
				if (damageAction != null)
				{
					damageActions.Add(damageAction);
				}
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06000597 RID: 1431 RVA: 0x0001245E File Offset: 0x0001065E
		protected virtual IEnumerable<BattleAction> PassiveActions()
		{
			if (this.HasMagic)
			{
				yield return new DollGainMagicAction(this, 1);
			}
			yield break;
		}

		// Token: 0x06000598 RID: 1432 RVA: 0x0001246E File Offset: 0x0001066E
		internal IEnumerable<BattleAction> GetActiveActions(List<DamageAction> damageActions)
		{
			foreach (BattleAction battleAction in this.ActiveActions())
			{
				DamageAction damageAction = battleAction as DamageAction;
				if (damageAction != null)
				{
					damageActions.Add(damageAction);
				}
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x00012485 File Offset: 0x00010685
		protected virtual IEnumerable<BattleAction> ActiveActions()
		{
			return null;
		}

		// Token: 0x0600059A RID: 1434 RVA: 0x00012488 File Offset: 0x00010688
		protected void NotifyPassiveActivating()
		{
			Action passiveActivating = this.PassiveActivating;
			if (passiveActivating == null)
			{
				return;
			}
			passiveActivating.Invoke();
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x0001249A File Offset: 0x0001069A
		protected void NotifyActiveActivating()
		{
			Action activeActivating = this.ActiveActivating;
			if (activeActivating == null)
			{
				return;
			}
			activeActivating.Invoke();
		}

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x0600059C RID: 1436 RVA: 0x000124AC File Offset: 0x000106AC
		// (remove) Token: 0x0600059D RID: 1437 RVA: 0x000124E4 File Offset: 0x000106E4
		public event Action PassiveActivating;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x0600059E RID: 1438 RVA: 0x0001251C File Offset: 0x0001071C
		// (remove) Token: 0x0600059F RID: 1439 RVA: 0x00012554 File Offset: 0x00010754
		public event Action ActiveActivating;

		// Token: 0x060005A0 RID: 1440 RVA: 0x00012589 File Offset: 0x00010789
		public bool ConsumeMagic(int magic)
		{
			this.Magic -= magic;
			if (this.Magic >= 0)
			{
				return true;
			}
			this.Magic = 0;
			return false;
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x060005A1 RID: 1441 RVA: 0x000125AC File Offset: 0x000107AC
		// (set) Token: 0x060005A2 RID: 1442 RVA: 0x000125B4 File Offset: 0x000107B4
		public TargetType TargetType { get; protected set; }

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x060005A3 RID: 1443 RVA: 0x000125BD File Offset: 0x000107BD
		// (set) Token: 0x060005A4 RID: 1444 RVA: 0x000125C5 File Offset: 0x000107C5
		public string GunName { get; protected set; }

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x060005A5 RID: 1445 RVA: 0x000125CE File Offset: 0x000107CE
		// (set) Token: 0x060005A6 RID: 1446 RVA: 0x000125D6 File Offset: 0x000107D6
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

		// Token: 0x060005A7 RID: 1447 RVA: 0x000125EE File Offset: 0x000107EE
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

		// Token: 0x060005A8 RID: 1448
		protected abstract IEnumerable<BattleAction> Actions(UnitSelector selector);

		// Token: 0x060005A9 RID: 1449 RVA: 0x0001260C File Offset: 0x0001080C
		public virtual IEnumerable<BattleAction> OnRemove()
		{
			return null;
		}

		// Token: 0x040002C1 RID: 705
		private int _magic;

		// Token: 0x040002C2 RID: 706
		private readonly GameEventHandlerHolder _ownerHandlerHolder = new GameEventHandlerHolder();

		// Token: 0x040002C3 RID: 707
		private readonly WeakReference<PlayerUnit> _owner = new WeakReference<PlayerUnit>(null);

		// Token: 0x040002C8 RID: 712
		private Unit _pendingTarget;

		// Token: 0x0200021E RID: 542
		private sealed class DollFormatWrapper : GameEntityFormatWrapper
		{
			// Token: 0x0600114A RID: 4426 RVA: 0x0002EDEA File Offset: 0x0002CFEA
			public DollFormatWrapper(Doll doll)
				: base(doll)
			{
				this._doll = doll;
			}

			// Token: 0x0600114B RID: 4427 RVA: 0x0002EDFA File Offset: 0x0002CFFA
			protected override object GetArgument(string key)
			{
				if (!(key == "OwnerName"))
				{
					return base.GetArgument(key);
				}
				PlayerUnit owner = this._doll.Owner;
				return ((owner != null) ? owner.GetName() : null) ?? UnitNameTable.GetDefaultOwnerName();
			}

			// Token: 0x0600114C RID: 4428 RVA: 0x0002EE34 File Offset: 0x0002D034
			protected override string FormatArgument(object arg, string format)
			{
				if (!(arg is DamageInfo))
				{
					return base.FormatArgument(arg, format);
				}
				DamageInfo damageInfo = (DamageInfo)arg;
				int num = (int)damageInfo.Damage;
				BattleController battle = this._doll.Battle;
				if (battle == null)
				{
					return GameEntityFormatWrapper.WrappedFormatNumber(num, num, format);
				}
				int num2 = battle.CalculateDamage(this._doll, battle.Player, null, damageInfo);
				return GameEntityFormatWrapper.WrappedFormatNumber(num, num2, format);
			}

			// Token: 0x0400082B RID: 2091
			private readonly Doll _doll;
		}
	}
}
