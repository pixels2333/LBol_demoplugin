using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LBoL.Core.Battle.BattleActionRecord;
using UnityEngine;
namespace LBoL.Core.Battle
{
	internal class ActionResolver
	{
		public ActionResolver(BattleController battle)
		{
			this._battle = battle;
		}
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
		private const int MaxStackDepth = 99;
		private readonly BattleController _battle;
		private List<Reactor> _reactors;
		private Exception _exception;
	}
}
