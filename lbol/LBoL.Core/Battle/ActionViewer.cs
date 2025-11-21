using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.Core.Battle
{
	// Token: 0x0200013E RID: 318
	public class ActionViewer
	{
		// Token: 0x06000C0C RID: 3084 RVA: 0x00021654 File Offset: 0x0001F854
		private static Type GetActionTypeForViewer(Type type)
		{
			Type type2;
			if (ActionViewer.ViewerTypeTable.TryGetValue(type, ref type2))
			{
				return type2;
			}
			ActionViewerTypeAttribute customAttribute = CustomAttributeExtensions.GetCustomAttribute<ActionViewerTypeAttribute>(type);
			if (customAttribute != null && customAttribute.Type != null)
			{
				type2 = customAttribute.Type;
				if (!type.IsSubclassOf(type2))
				{
					throw new InvalidOperationException(string.Format("Cannot set {0} as {1}'s viewer type", type2, type));
				}
			}
			else
			{
				type2 = type;
			}
			ActionViewer.ViewerTypeTable.Add(type, type2);
			return type2;
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x000216B8 File Offset: 0x0001F8B8
		public void Register<TAction>(BattleActionViewer<TAction> viewer, Predicate<TAction> filter = null) where TAction : BattleAction
		{
			ActionViewer.ViewerEntryBase viewerEntryBase;
			ActionViewer.ViewerEntry<TAction> viewerEntry;
			if (this._viewerDict.TryGetValue(typeof(TAction), ref viewerEntryBase))
			{
				viewerEntry = (ActionViewer.ViewerEntry<TAction>)viewerEntryBase;
			}
			else
			{
				viewerEntry = new ActionViewer.ViewerEntry<TAction>();
				this._viewerDict.Add(typeof(TAction), viewerEntry);
			}
			viewerEntry.Register(viewer, filter);
		}

		// Token: 0x06000C0E RID: 3086 RVA: 0x0002170C File Offset: 0x0001F90C
		public void Unregister<TAction>(BattleActionViewer<TAction> viewer) where TAction : BattleAction
		{
			ActionViewer.ViewerEntryBase viewerEntryBase;
			if (this._viewerDict.TryGetValue(typeof(TAction), ref viewerEntryBase))
			{
				((ActionViewer.ViewerEntry<TAction>)viewerEntryBase).Unregister(viewer);
			}
		}

		// Token: 0x06000C0F RID: 3087 RVA: 0x00021740 File Offset: 0x0001F940
		internal IEnumerator View(BattleAction action)
		{
			ActionViewer.ViewerEntryBase viewerEntryBase;
			if (!this._viewerDict.TryGetValue(ActionViewer.GetActionTypeForViewer(action.GetType()), ref viewerEntryBase))
			{
				return null;
			}
			return viewerEntryBase.DynamicView(action);
		}

		// Token: 0x04000581 RID: 1409
		private static readonly Dictionary<Type, Type> ViewerTypeTable = new Dictionary<Type, Type>();

		// Token: 0x04000582 RID: 1410
		private readonly Dictionary<Type, ActionViewer.ViewerEntryBase> _viewerDict = new Dictionary<Type, ActionViewer.ViewerEntryBase>();

		// Token: 0x02000293 RID: 659
		private abstract class ViewerEntryBase
		{
			// Token: 0x060013C5 RID: 5061
			public abstract IEnumerator DynamicView(BattleAction action);
		}

		// Token: 0x02000294 RID: 660
		private class ViewerEntry<TAction> : ActionViewer.ViewerEntryBase where TAction : BattleAction
		{
			// Token: 0x060013C7 RID: 5063 RVA: 0x0003660A File Offset: 0x0003480A
			public void Register(BattleActionViewer<TAction> viewer, Predicate<TAction> filter)
			{
				this._viewers.Add(new ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>>(viewer, filter));
			}

			// Token: 0x060013C8 RID: 5064 RVA: 0x00036620 File Offset: 0x00034820
			public void Unregister(BattleActionViewer<TAction> viewer)
			{
				this._viewers.RemoveAll(([TupleElementNames(new string[] { "viewer", "filter" })] ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>> pair) => pair.Item1 == viewer);
			}

			// Token: 0x060013C9 RID: 5065 RVA: 0x00036654 File Offset: 0x00034854
			public override IEnumerator DynamicView(BattleAction action)
			{
				TAction taction = action as TAction;
				if (taction == null)
				{
					throw new InvalidOperationException("ActionViewer<" + typeof(TAction).Name + "> views unmatched " + action.GetType().Name);
				}
				foreach (ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>> valueTuple in this._viewers)
				{
					BattleActionViewer<TAction> item = valueTuple.Item1;
					Predicate<TAction> item2 = valueTuple.Item2;
					if (item2 == null || item2.Invoke(taction))
					{
						IEnumerator enumerator2 = item(taction);
						if (enumerator2 != null)
						{
							return enumerator2;
						}
					}
				}
				return null;
			}

			// Token: 0x04000A5A RID: 2650
			[TupleElementNames(new string[] { "viewer", "filter" })]
			private readonly List<ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>>> _viewers = new List<ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>>>();
		}
	}
}
