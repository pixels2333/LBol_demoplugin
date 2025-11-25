using System;
using System.Collections.Generic;
namespace LBoL.Core
{
	public sealed class GameEvent<T> where T : GameEventArgs
	{
		internal void AddHandler(GameEventHandler<T> handler, Predicate<T> filter, GameEventPriority priority)
		{
			if (handler == null)
			{
				throw new ArgumentNullException("handler");
			}
			List<GameEvent<T>.HandlerEntry> list;
			if (!this._invocationListDict.TryGetValue(priority, ref list))
			{
				list = new List<GameEvent<T>.HandlerEntry>();
				this._invocationListDict.Add(priority, list);
			}
			list.Add(new GameEvent<T>.HandlerEntry
			{
				Handler = handler,
				Filter = filter
			});
		}
		public void AddHandler(GameEventHandler<T> handler, GameEventPriority priority)
		{
			this.AddHandler(handler, null, priority);
		}
		public void RemoveHandler(GameEventHandler<T> handler, GameEventPriority priority)
		{
			List<GameEvent<T>.HandlerEntry> list;
			if (this._invocationListDict.TryGetValue(priority, ref list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Handler == handler)
					{
						list.RemoveAt(i);
						return;
					}
				}
			}
		}
		internal void Execute(T args)
		{
			if (args.IsCanceled)
			{
				return;
			}
			foreach (KeyValuePair<GameEventPriority, List<GameEvent<T>.HandlerEntry>> keyValuePair in this._invocationListDict)
			{
				GameEventPriority gameEventPriority;
				List<GameEvent<T>.HandlerEntry> list;
				keyValuePair.Deconstruct(ref gameEventPriority, ref list);
				foreach (GameEvent<T>.HandlerEntry handlerEntry in list)
				{
					if (handlerEntry.Filter == null || handlerEntry.Filter.Invoke(args))
					{
						handlerEntry.Handler(args);
						if (args.IsCanceled)
						{
							return;
						}
					}
				}
			}
		}
		private readonly SortedDictionary<GameEventPriority, List<GameEvent<T>.HandlerEntry>> _invocationListDict = new SortedDictionary<GameEventPriority, List<GameEvent<T>.HandlerEntry>>();
		private class HandlerEntry
		{
			public GameEventHandler<T> Handler { get; set; }
			public Predicate<T> Filter { get; set; }
		}
	}
}
