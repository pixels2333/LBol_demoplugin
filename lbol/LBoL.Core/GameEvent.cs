using System;
using System.Collections.Generic;

namespace LBoL.Core
{
	// Token: 0x02000017 RID: 23
	public sealed class GameEvent<T> where T : GameEventArgs
	{
		// Token: 0x060000CE RID: 206 RVA: 0x00003910 File Offset: 0x00001B10
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

		// Token: 0x060000CF RID: 207 RVA: 0x00003967 File Offset: 0x00001B67
		public void AddHandler(GameEventHandler<T> handler, GameEventPriority priority)
		{
			this.AddHandler(handler, null, priority);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00003974 File Offset: 0x00001B74
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

		// Token: 0x060000D1 RID: 209 RVA: 0x000039C0 File Offset: 0x00001BC0
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

		// Token: 0x04000084 RID: 132
		private readonly SortedDictionary<GameEventPriority, List<GameEvent<T>.HandlerEntry>> _invocationListDict = new SortedDictionary<GameEventPriority, List<GameEvent<T>.HandlerEntry>>();

		// Token: 0x020001CD RID: 461
		private class HandlerEntry
		{
			// Token: 0x17000563 RID: 1379
			// (get) Token: 0x06001003 RID: 4099 RVA: 0x0002AF7C File Offset: 0x0002917C
			// (set) Token: 0x06001004 RID: 4100 RVA: 0x0002AF84 File Offset: 0x00029184
			public GameEventHandler<T> Handler { get; set; }

			// Token: 0x17000564 RID: 1380
			// (get) Token: 0x06001005 RID: 4101 RVA: 0x0002AF8D File Offset: 0x0002918D
			// (set) Token: 0x06001006 RID: 4102 RVA: 0x0002AF95 File Offset: 0x00029195
			public Predicate<T> Filter { get; set; }
		}
	}
}
