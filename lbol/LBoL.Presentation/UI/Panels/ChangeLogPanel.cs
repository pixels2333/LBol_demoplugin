using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200008E RID: 142
	public class ChangeLogPanel : UiPanel
	{
		// Token: 0x06000788 RID: 1928 RVA: 0x000234CF File Offset: 0x000216CF
		protected override void OnShown()
		{
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x000234D1 File Offset: 0x000216D1
		protected override void OnHided()
		{
		}

		// Token: 0x0600078A RID: 1930 RVA: 0x000234D3 File Offset: 0x000216D3
		private void Awake()
		{
			base.GetComponent<Button>().onClick.AddListener(new UnityAction(base.Hide));
		}
	}
}
