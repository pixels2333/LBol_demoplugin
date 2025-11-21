using System;
using System.Collections.Generic;

namespace LBoL.Core
{
	// Token: 0x0200003E RID: 62
	public class GameEventHandlerHolder
	{
		// Token: 0x060001D9 RID: 473 RVA: 0x00004B80 File Offset: 0x00002D80
		public void HandleEvent<T>(GameEvent<T> e, GameEventHandler<T> action, GameEventPriority priority) where T : GameEventArgs
		{
			e.AddHandler(action, priority);
			this._removeHandlerFunctions.Add(delegate
			{
				e.RemoveHandler(action, priority);
			});
		}

		// Token: 0x060001DA RID: 474 RVA: 0x00004BD8 File Offset: 0x00002DD8
		public void ClearEventHandlers()
		{
			foreach (Action action in this._removeHandlerFunctions)
			{
				action.Invoke();
			}
		}

		// Token: 0x04000100 RID: 256
		private readonly List<Action> _removeHandlerFunctions = new List<Action>();
	}
}
