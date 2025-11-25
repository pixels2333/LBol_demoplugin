using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
namespace LBoL.Core
{
	public class GameEventArgs
	{
		protected static string DebugString(GameEntity entity)
		{
			if (entity != null)
			{
				return entity.Name ?? entity.Id;
			}
			return "<null>";
		}
		protected static string DebugString(UnitSelector selector)
		{
			if (selector == null)
			{
				return "<null>";
			}
			if (selector.Type != TargetType.SingleEnemy)
			{
				return selector.Type.ToString();
			}
			return GameEventArgs.DebugString(selector.SelectedEnemy);
		}
		protected static string DebugString(StatusEffect effect)
		{
			if (effect != null)
			{
				string text = effect.Name ?? effect.Id;
				StringBuilder stringBuilder = new StringBuilder().Append('{').Append(text);
				if (effect.HasLevel)
				{
					stringBuilder.Append(" L: ").Append(effect.Level);
				}
				if (effect.HasDuration)
				{
					stringBuilder.Append(" D: ").Append(effect.Duration);
				}
				if (effect.HasCount)
				{
					stringBuilder.Append(" C: ").Append(effect.Count).Append("/")
						.Append(effect.Limit);
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}
			return "<null>";
		}
		public bool IsModified { get; internal set; }
		public GameEntity ActionSource { get; internal set; }
		public ActionCause Cause { get; internal set; }
		public bool CanCancel { get; internal set; } = true;
		public bool IsCanceled
		{
			get
			{
				return this._isCanceled;
			}
			private set
			{
				if (!this.CanCancel)
				{
					throw new InvalidOperationException(base.GetType().Name + " cannot be canceled.");
				}
				this._isCanceled = value;
			}
		}
		public CancelCause CancelCause { get; private set; }
		public void CancelBy([NotNull] GameEntity source)
		{
			this.IsCanceled = true;
			this.CanCancel = false;
			this.CancelCause = CancelCause.Reaction;
			this._cancelSource.SetTarget(source);
		}
		internal void ForceCancelBecause(CancelCause cause)
		{
			this._isCanceled = true;
			this.CanCancel = false;
			this.CancelCause = cause;
		}
		public GameEntity CancelSource
		{
			get
			{
				GameEntity gameEntity;
				if (!this._cancelSource.TryGetTarget(ref gameEntity))
				{
					return null;
				}
				return gameEntity;
			}
		}
		internal string[] Modifiers
		{
			get
			{
				List<string> list = new List<string>();
				using (List<WeakReference<GameEntity>>.Enumerator enumerator = this._modifiers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						GameEntity gameEntity;
						if (enumerator.Current.TryGetTarget(ref gameEntity))
						{
							list.Add(gameEntity.Name);
						}
					}
				}
				return list.ToArray();
			}
		}
		public void AddModifier(GameEntity entity)
		{
			this._modifiers.Add(new WeakReference<GameEntity>(entity));
			this.IsModified = true;
		}
		internal void ClearModifiers()
		{
			this._cancelSource.SetTarget(null);
			this.IsModified = false;
			this._modifiers.Clear();
		}
		protected virtual string GetBaseDebugString()
		{
			return string.Empty;
		}
		[return: MaybeNull]
		public string ExportDebugDetails()
		{
			return this.GetBaseDebugString();
		}
		private const string DebugNullStr = "<null>";
		private bool _isCanceled;
		private readonly WeakReference<GameEntity> _cancelSource = new WeakReference<GameEntity>(null);
		private readonly List<WeakReference<GameEntity>> _modifiers = new List<WeakReference<GameEntity>>();
	}
}
