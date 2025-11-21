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
	// Token: 0x02000019 RID: 25
	public class GameEventArgs
	{
		// Token: 0x060000D3 RID: 211 RVA: 0x00003AA7 File Offset: 0x00001CA7
		protected static string DebugString(GameEntity entity)
		{
			if (entity != null)
			{
				return entity.Name ?? entity.Id;
			}
			return "<null>";
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00003AC4 File Offset: 0x00001CC4
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

		// Token: 0x060000D5 RID: 213 RVA: 0x00003B04 File Offset: 0x00001D04
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

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x00003BC2 File Offset: 0x00001DC2
		// (set) Token: 0x060000D7 RID: 215 RVA: 0x00003BCA File Offset: 0x00001DCA
		public bool IsModified { get; internal set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x00003BD3 File Offset: 0x00001DD3
		// (set) Token: 0x060000D9 RID: 217 RVA: 0x00003BDB File Offset: 0x00001DDB
		public GameEntity ActionSource { get; internal set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000DA RID: 218 RVA: 0x00003BE4 File Offset: 0x00001DE4
		// (set) Token: 0x060000DB RID: 219 RVA: 0x00003BEC File Offset: 0x00001DEC
		public ActionCause Cause { get; internal set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000DC RID: 220 RVA: 0x00003BF5 File Offset: 0x00001DF5
		// (set) Token: 0x060000DD RID: 221 RVA: 0x00003BFD File Offset: 0x00001DFD
		public bool CanCancel { get; internal set; } = true;

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x060000DE RID: 222 RVA: 0x00003C06 File Offset: 0x00001E06
		// (set) Token: 0x060000DF RID: 223 RVA: 0x00003C0E File Offset: 0x00001E0E
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

		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00003C3A File Offset: 0x00001E3A
		// (set) Token: 0x060000E1 RID: 225 RVA: 0x00003C42 File Offset: 0x00001E42
		public CancelCause CancelCause { get; private set; }

		// Token: 0x060000E2 RID: 226 RVA: 0x00003C4B File Offset: 0x00001E4B
		public void CancelBy([NotNull] GameEntity source)
		{
			this.IsCanceled = true;
			this.CanCancel = false;
			this.CancelCause = CancelCause.Reaction;
			this._cancelSource.SetTarget(source);
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x00003C72 File Offset: 0x00001E72
		internal void ForceCancelBecause(CancelCause cause)
		{
			this._isCanceled = true;
			this.CanCancel = false;
			this.CancelCause = cause;
		}

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000E4 RID: 228 RVA: 0x00003C8C File Offset: 0x00001E8C
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

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00003CAC File Offset: 0x00001EAC
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

		// Token: 0x060000E6 RID: 230 RVA: 0x00003D18 File Offset: 0x00001F18
		public void AddModifier(GameEntity entity)
		{
			this._modifiers.Add(new WeakReference<GameEntity>(entity));
			this.IsModified = true;
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x00003D32 File Offset: 0x00001F32
		internal void ClearModifiers()
		{
			this._cancelSource.SetTarget(null);
			this.IsModified = false;
			this._modifiers.Clear();
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00003D52 File Offset: 0x00001F52
		protected virtual string GetBaseDebugString()
		{
			return string.Empty;
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00003D59 File Offset: 0x00001F59
		[return: MaybeNull]
		public string ExportDebugDetails()
		{
			return this.GetBaseDebugString();
		}

		// Token: 0x04000091 RID: 145
		private const string DebugNullStr = "<null>";

		// Token: 0x04000096 RID: 150
		private bool _isCanceled;

		// Token: 0x04000097 RID: 151
		private readonly WeakReference<GameEntity> _cancelSource = new WeakReference<GameEntity>(null);

		// Token: 0x04000099 RID: 153
		private readonly List<WeakReference<GameEntity>> _modifiers = new List<WeakReference<GameEntity>>();
	}
}
