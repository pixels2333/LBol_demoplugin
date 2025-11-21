using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Battle;

namespace LBoL.Core
{
	// Token: 0x02000053 RID: 83
	public class InteractionViewer
	{
		// Token: 0x06000375 RID: 885 RVA: 0x0000B6FC File Offset: 0x000098FC
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

		// Token: 0x06000376 RID: 886 RVA: 0x0000B748 File Offset: 0x00009948
		public void Unregister<T>(InteractionViewer<T> viewer) where T : Interaction
		{
			List<Delegate> list;
			if (this._viewerDict.TryGetValue(typeof(T), ref list))
			{
				list.Remove(viewer);
			}
		}

		// Token: 0x06000377 RID: 887 RVA: 0x0000B778 File Offset: 0x00009978
		public IEnumerator View(Interaction interaction)
		{
			List<Delegate> list;
			if (!this._viewerDict.TryGetValue(interaction.GetType(), ref list))
			{
				return null;
			}
			return Enumerable.FirstOrDefault<IEnumerator>(Enumerable.Select<Delegate, IEnumerator>(list, (Delegate viewer) => (IEnumerator)viewer.Method.Invoke(viewer.Target, new object[] { interaction })), (IEnumerator i) => i != null);
		}

		// Token: 0x0400020A RID: 522
		private readonly Dictionary<Type, List<Delegate>> _viewerDict = new Dictionary<Type, List<Delegate>>();
	}
}
