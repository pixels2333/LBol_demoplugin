using System;
using System.Collections.Generic;
namespace LBoL.Core
{
	public class GameEventHandlerHolder
	{
		public void HandleEvent<T>(GameEvent<T> e, GameEventHandler<T> action, GameEventPriority priority) where T : GameEventArgs
		{
			e.AddHandler(action, priority);
			this._removeHandlerFunctions.Add(delegate
			{
				e.RemoveHandler(action, priority);
			});
		}
		public void ClearEventHandlers()
		{
			foreach (Action action in this._removeHandlerFunctions)
			{
				action.Invoke();
			}
		}
		private readonly List<Action> _removeHandlerFunctions = new List<Action>();
	}
}
