using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle
{
	public class Reactor
	{
		public GameEntity Source { get; internal set; }
		public ActionCause Cause { get; internal set; }
		public static implicit operator Reactor(BattleAction action)
		{
			return new Reactor(action);
		}
		public static implicit operator Reactor(LazyActionReactor lazyActionReactor)
		{
			return new Reactor(lazyActionReactor);
		}
		public static implicit operator Reactor(LazySequencedReactor lazySequencedReactor)
		{
			return new Reactor(lazySequencedReactor);
		}
		public Reactor(BattleAction action)
		{
			this._underlyingReactor = action;
		}
		public Reactor(IEnumerable<BattleAction> action)
		{
			this._underlyingReactor = action;
		}
		public Reactor(LazyActionReactor action)
		{
			this._underlyingReactor = action;
		}
		public Reactor(LazySequencedReactor sequencedReactor)
		{
			this._underlyingReactor = sequencedReactor;
		}
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
		private readonly object _underlyingReactor;
	}
}
