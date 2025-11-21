using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LBoL.Core.Battle.BattleActionRecord;
using UnityEngine;

namespace LBoL.Core.Battle
{
	// Token: 0x0200013C RID: 316
	internal class ActionResolver
	{
		// Token: 0x06000C04 RID: 3076 RVA: 0x000215CC File Offset: 0x0001F7CC
		public ActionResolver(BattleController battle)
		{
			this._battle = battle;
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x000215DB File Offset: 0x0001F7DB
		public IEnumerator<object> Resolve(BattleAction root, [MaybeNull] string recordName = null)
		{
			if (this._exception != null)
			{
				Debug.LogError("Resolver state not cleared.");
			}
			PhaseRecord rootPhaseRecord = new PhaseRecord("FakeRoot", "Fake root phase", false, Array.Empty<string>(), false, CancelCause.None, null, null);
			yield return this.InternalResolve(rootPhaseRecord, root, 0, recordName);
			ActionRecord.TriggerResolved(Enumerable.Single<ActionRecord>(rootPhaseRecord.Reactions));
			if (this._exception != null)
			{
				Exception exception = this._exception;
				this._exception = null;
				Debug.LogException(exception);
			}
			yield break;
		}

		// Token: 0x06000C06 RID: 3078 RVA: 0x000215F8 File Offset: 0x0001F7F8
		private IEnumerator<object> InternalResolve(PhaseRecord parentPhase, BattleAction action, int depth, [MaybeNull] string recordName = null)
		{
			if (action.Resolved)
			{
				throw new InvalidOperationException(string.Format("{0} has already been resolved. (Reenter)", action));
			}
			action.Resolved = true;
			if (depth > 99)
			{
				Debug.LogError(string.Format("ActionResolver stack depth over {0}, reactions regarded", 99));
				yield break;
			}
			ActionRecord actionRecord = new ActionRecord(action, recordName);
			action.Battle = this._battle;
			foreach (Phase phase in action.GetPhases())
			{
				if (this._reactors != null)
				{
					Debug.LogError("Reactors not cleared.");
				}
				this._reactors = new List<Reactor>();
				yield return phase.Flow();
				List<Reactor> reactors = this._reactors;
				this._reactors = null;
				PhaseRecord phaseRecord = new PhaseRecord(phase);
				actionRecord.Phases.Add(phaseRecord);
				if (phase.Exception != null)
				{
					this._exception = phase.Exception;
					break;
				}
				foreach (Reactor reactor in reactors)
				{
					using (IEnumerator<BattleAction> enumerator = reactor.EnumerateReactions().GetEnumerator())
					{
						do
						{
							try
							{
								if (!enumerator.MoveNext())
								{
									break;
								}
							}
							catch (Exception ex)
							{
								this._exception = ex;
								break;
							}
							BattleAction battleAction = enumerator.Current;
							if (battleAction == null)
							{
								yield return null;
							}
							else
							{
								yield return this.InternalResolve(phaseRecord, battleAction.SetSource(reactor.Source).SetCause(reactor.Cause), depth + 1, null);
							}
						}
						while (this._exception == null);
					}
					IEnumerator<BattleAction> enumerator = null;
					if (this._exception != null)
					{
						break;
					}
					reactor = null;
				}
				List<Reactor>.Enumerator enumerator3 = default(List<Reactor>.Enumerator);
				if (phase.IsCanceled)
				{
					break;
				}
				phaseRecord = null;
				phase = null;
			}
			IEnumerator<Phase> enumerator2 = null;
			parentPhase.Reactions.Add(actionRecord);
			yield break;
			yield break;
		}

		// Token: 0x06000C07 RID: 3079 RVA: 0x00021624 File Offset: 0x0001F824
		public void React(Reactor reactor, [MaybeNull] GameEntity source, ActionCause cause)
		{
			if (this._reactors == null)
			{
				throw new InvalidOperationException("Reacting out of action-resolving status");
			}
			reactor.Source = source;
			reactor.Cause = cause;
			this._reactors.Add(reactor);
		}

		// Token: 0x0400057D RID: 1405
		private const int MaxStackDepth = 99;

		// Token: 0x0400057E RID: 1406
		private readonly BattleController _battle;

		// Token: 0x0400057F RID: 1407
		private List<Reactor> _reactors;

		// Token: 0x04000580 RID: 1408
		private Exception _exception;
	}
}
