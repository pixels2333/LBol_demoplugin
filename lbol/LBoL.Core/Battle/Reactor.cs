using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle
{
	// Token: 0x0200014B RID: 331
	public class Reactor
	{
		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x06000D3E RID: 3390 RVA: 0x0002534A File Offset: 0x0002354A
		// (set) Token: 0x06000D3F RID: 3391 RVA: 0x00025352 File Offset: 0x00023552
		public GameEntity Source { get; internal set; }

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x06000D40 RID: 3392 RVA: 0x0002535B File Offset: 0x0002355B
		// (set) Token: 0x06000D41 RID: 3393 RVA: 0x00025363 File Offset: 0x00023563
		public ActionCause Cause { get; internal set; }

		// Token: 0x06000D42 RID: 3394 RVA: 0x0002536C File Offset: 0x0002356C
		public static implicit operator Reactor(BattleAction action)
		{
			return new Reactor(action);
		}

		// Token: 0x06000D43 RID: 3395 RVA: 0x00025374 File Offset: 0x00023574
		public static implicit operator Reactor(LazyActionReactor lazyActionReactor)
		{
			return new Reactor(lazyActionReactor);
		}

		// Token: 0x06000D44 RID: 3396 RVA: 0x0002537C File Offset: 0x0002357C
		public static implicit operator Reactor(LazySequencedReactor lazySequencedReactor)
		{
			return new Reactor(lazySequencedReactor);
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x00025384 File Offset: 0x00023584
		public Reactor(BattleAction action)
		{
			this._underlyingReactor = action;
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x00025393 File Offset: 0x00023593
		public Reactor(IEnumerable<BattleAction> action)
		{
			this._underlyingReactor = action;
		}

		// Token: 0x06000D47 RID: 3399 RVA: 0x000253A2 File Offset: 0x000235A2
		public Reactor(LazyActionReactor action)
		{
			this._underlyingReactor = action;
		}

		// Token: 0x06000D48 RID: 3400 RVA: 0x000253B1 File Offset: 0x000235B1
		public Reactor(LazySequencedReactor sequencedReactor)
		{
			this._underlyingReactor = sequencedReactor;
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x000253C0 File Offset: 0x000235C0
		internal IEnumerable<BattleAction> EnumerateReactions()
		{
			object underlyingReactor = this._underlyingReactor;
			BattleAction battleAction = underlyingReactor as BattleAction;
			if (battleAction == null)
			{
				IEnumerable<BattleAction> enumerable = underlyingReactor as IEnumerable<BattleAction>;
				if (enumerable == null)
				{
					LazyActionReactor lazyActionReactor = underlyingReactor as LazyActionReactor;
					if (lazyActionReactor == null)
					{
						LazySequencedReactor lazySequencedReactor = underlyingReactor as LazySequencedReactor;
						if (lazySequencedReactor != null)
						{
							foreach (BattleAction battleAction2 in lazySequencedReactor())
							{
								yield return battleAction2;
							}
							IEnumerator<BattleAction> enumerator = null;
						}
					}
					else
					{
						yield return lazyActionReactor();
					}
				}
				else
				{
					foreach (BattleAction battleAction3 in enumerable)
					{
						yield return battleAction3;
					}
					IEnumerator<BattleAction> enumerator = null;
				}
			}
			else
			{
				yield return battleAction;
			}
			yield break;
			yield break;
		}

		// Token: 0x0400062C RID: 1580
		private readonly object _underlyingReactor;
	}
}
