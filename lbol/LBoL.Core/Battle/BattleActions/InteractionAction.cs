using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class InteractionAction : BattleAction
	{
		public InteractionAction(Interaction interaction, bool canCancel = false)
		{
			this._interaction = interaction;
			this._interaction.CanCancel = canCancel;
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Resolve", base.Battle.GameRun.InteractionViewer.View(this._interaction), false);
			yield break;
		}
		public override bool IsModified
		{
			get
			{
				return false;
			}
		}
		public override string[] Modifiers
		{
			get
			{
				return Array.Empty<string>();
			}
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}
		public override void ClearModifiers()
		{
		}
		public override string Name
		{
			get
			{
				return base.Name + " (" + this._interaction.GetType().Name + ")";
			}
		}
		public override string ExportDebugDetails()
		{
			return string.Empty;
		}
		private readonly Interaction _interaction;
	}
}
