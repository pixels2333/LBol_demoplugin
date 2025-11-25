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
	[Localizable]
	public abstract class UltimateSkill : GameEntity
	{
		protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
		{
			return TypeFactory<UltimateSkill>.LocalizeProperty(base.Id, key, decorated, required);
		}
		protected virtual IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
		{
			return TypeFactory<UltimateSkill>.LocalizeListProperty(base.Id, key, required);
		}
		internal override GameEventPriority DefaultEventPriority
		{
			get
			{
				return (GameEventPriority)this.Config.Order;
			}
		}
		public UltimateSkillConfig Config { get; private set; }
		public override string DebugName
		{
			get
			{
				return string.Concat(new string[] { "<", base.Id, ">(", this.Title, "-", this.Content, ")" });
			}
		}
		public override string Name
		{
			get
			{
				return this.Title + "-" + this.Content;
			}
		}
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
		public int PowerCost
		{
			get
			{
				return this.Config.PowerCost;
			}
		}
		public bool TurnRepeatable
		{
			get
			{
				return this.UsRepeatableType == UsRepeatableType.FreeToUse;
			}
		}
		public bool BattleRepeatable
		{
			get
			{
				UsRepeatableType usRepeatableType = this.UsRepeatableType;
				return usRepeatableType == UsRepeatableType.FreeToUse || usRepeatableType == UsRepeatableType.OncePerTurn;
			}
		}
		public UsRepeatableType UsRepeatableType { get; set; }
		public string GunName { get; protected set; }
		public int PowerPerLevel
		{
			get
			{
				return this.Config.PowerPerLevel;
			}
		}
		public int MaxPowerLevel { get; set; }
		[UsedImplicitly]
		public virtual DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)this.Config.Damage, true);
			}
		}
		[UsedImplicitly]
		public int Value1
		{
			get
			{
				return this.Config.Value1;
			}
		}
		[UsedImplicitly]
		public int Value2
		{
			get
			{
				return this.Config.Value2;
			}
		}
		public string Title
		{
			get
			{
				return this.LocalizeProperty("Title", false, false);
			}
		}
		public string Content
		{
			get
			{
				return this.LocalizeProperty("Content", false, false);
			}
		}
		public bool TurnAvailable { get; internal set; }
		public bool BattleAvailable { get; internal set; }
		public bool Available
		{
			get
			{
				return this.TurnAvailable && this.BattleAvailable;
			}
		}
		public TargetType TargetType { get; protected set; }
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
		internal override GameEntityFormatWrapper CreateFormatWrapper()
		{
			return new UltimateSkill.UsFormatWrapper(this);
		}
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
		protected abstract IEnumerable<BattleAction> Actions(UnitSelector selector);
		public IEnumerable<IDisplayWord> EnumerateDisplayWords(bool verbose)
		{
			return Library.InternalEnumerateDisplayWords(base.GameRun, this.Config.Keywords, this.Config.RelativeEffects, verbose, default(Keyword?));
		}
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
		private readonly WeakReference<PlayerUnit> _owner = new WeakReference<PlayerUnit>(null);
		private Unit _pendingTarget;
		internal sealed class UsFormatWrapper : GameEntityFormatWrapper
		{
			public UsFormatWrapper(UltimateSkill us)
				: base(us)
			{
				this._us = us;
			}
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
			private readonly UltimateSkill _us;
		}
	}
}
