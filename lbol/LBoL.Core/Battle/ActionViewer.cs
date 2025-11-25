using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.Core.Battle
{
	public class ActionViewer
	{
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
		public void Unregister<TAction>(BattleActionViewer<TAction> viewer) where TAction : BattleAction
		{
			ActionViewer.ViewerEntryBase viewerEntryBase;
			if (this._viewerDict.TryGetValue(typeof(TAction), ref viewerEntryBase))
			{
				((ActionViewer.ViewerEntry<TAction>)viewerEntryBase).Unregister(viewer);
			}
		}
		internal IEnumerator View(BattleAction action)
		{
			ActionViewer.ViewerEntryBase viewerEntryBase;
			if (!this._viewerDict.TryGetValue(ActionViewer.GetActionTypeForViewer(action.GetType()), ref viewerEntryBase))
			{
				return null;
			}
			return viewerEntryBase.DynamicView(action);
		}
		private static readonly Dictionary<Type, Type> ViewerTypeTable = new Dictionary<Type, Type>();
		private readonly Dictionary<Type, ActionViewer.ViewerEntryBase> _viewerDict = new Dictionary<Type, ActionViewer.ViewerEntryBase>();
		private abstract class ViewerEntryBase
		{
			public abstract IEnumerator DynamicView(BattleAction action);
		}
		private class ViewerEntry<TAction> : ActionViewer.ViewerEntryBase where TAction : BattleAction
		{
			public void Register(BattleActionViewer<TAction> viewer, Predicate<TAction> filter)
			{
				this._viewers.Add(new ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>>(viewer, filter));
			}
			public void Unregister(BattleActionViewer<TAction> viewer)
			{
				this._viewers.RemoveAll(([TupleElementNames(new string[] { "viewer", "filter" })] ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>> pair) => pair.Item1 == viewer);
			}
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
			[TupleElementNames(new string[] { "viewer", "filter" })]
			private readonly List<ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>>> _viewers = new List<ValueTuple<BattleActionViewer<TAction>, Predicate<TAction>>>();
		}
	}
}
