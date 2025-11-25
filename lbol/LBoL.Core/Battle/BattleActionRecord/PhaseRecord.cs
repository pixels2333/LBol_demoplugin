using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base.Extensions;
using UnityEngine;
namespace LBoL.Core.Battle.BattleActionRecord
{
	public class PhaseRecord
	{
		public List<ActionRecord> Reactions { get; } = new List<ActionRecord>();
		public string Name { get; }
		public string Details { get; }
		public bool IsModified { get; }
		public string[] Modifiers { get; }
		public bool IsCanceled { get; }
		public CancelCause CancelCause { get; }
		public string CancelSource { get; }
		public string ExceptionString
		{
			[return: MaybeNull]
			get;
		}
		internal PhaseRecord(string name, string details, bool isModified, string[] modifiers, bool isCanceled, CancelCause cancelCause, [MaybeNull] GameEntity cancelSource = null, [MaybeNull] Exception exception = null)
		{
			this.Name = name;
			this.Details = details;
			this.IsModified = isModified;
			this.Modifiers = modifiers;
			this.IsCanceled = isCanceled;
			this.CancelCause = cancelCause;
			this.CancelSource = ((cancelSource != null) ? cancelSource.Name : null);
			this.ExceptionString = ((exception != null) ? exception.Message : null);
		}
		internal PhaseRecord(Phase phase)
			: this(phase.Name, phase.DebugDetails, phase.IsModified, phase.Modifiers, phase.IsCanceled, phase.CancelCause, phase.CancelSource, phase.Exception)
		{
			if (phase.Name.IsNullOrWhiteSpace())
			{
				this.Name = "<Unnamed>";
				Debug.LogWarning("Unnamed phase in " + phase.Action.Name);
			}
			phase.ClearModifiers();
		}
	}
}
