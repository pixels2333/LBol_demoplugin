using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Battle;
namespace LBoL.Core
{
	public class InteractionViewer
	{
		public void Register<T>(InteractionViewer<T> viewer) where T : Interaction
		{
			List<Delegate> list;
			if (!this._viewerDict.TryGetValue(typeof(T), ref list))
			{
				list = new List<Delegate>();
				this._viewerDict.Add(typeof(T), list);
			}
			list.Add(viewer);
		}
		public void Unregister<T>(InteractionViewer<T> viewer) where T : Interaction
		{
			List<Delegate> list;
			if (this._viewerDict.TryGetValue(typeof(T), ref list))
			{
				list.Remove(viewer);
			}
		}
		public IEnumerator View(Interaction interaction)
		{
			List<Delegate> list;
			if (!this._viewerDict.TryGetValue(interaction.GetType(), ref list))
			{
				return null;
			}
			return Enumerable.FirstOrDefault<IEnumerator>(Enumerable.Select<Delegate, IEnumerator>(list, (Delegate viewer) => (IEnumerator)viewer.Method.Invoke(viewer.Target, new object[] { interaction })), (IEnumerator i) => i != null);
		}
		private readonly Dictionary<Type, List<Delegate>> _viewerDict = new Dictionary<Type, List<Delegate>>();
	}
}
