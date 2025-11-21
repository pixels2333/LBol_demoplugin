using System;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000041 RID: 65
	public class CardFlyBrief : MonoBehaviour
	{
		// Token: 0x170000AE RID: 174
		// (get) Token: 0x06000420 RID: 1056 RVA: 0x00010A7D File Offset: 0x0000EC7D
		// (set) Token: 0x06000421 RID: 1057 RVA: 0x00010A85 File Offset: 0x0000EC85
		private bool Updating { get; set; }

		// Token: 0x170000AF RID: 175
		// (get) Token: 0x06000423 RID: 1059 RVA: 0x00010A97 File Offset: 0x0000EC97
		// (set) Token: 0x06000422 RID: 1058 RVA: 0x00010A8E File Offset: 0x0000EC8E
		private Vector2 LastPosition { get; set; }

		// Token: 0x06000424 RID: 1060 RVA: 0x00010A9F File Offset: 0x0000EC9F
		private void Start()
		{
			this.LastPosition = base.transform.position;
			this.Updating = true;
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x00010AC0 File Offset: 0x0000ECC0
		private void Update()
		{
			if (this.Updating)
			{
				Vector2 vector = base.transform.position - this.LastPosition;
				float num = Vector2.SignedAngle(Vector2.up, vector);
				this.card.transform.localRotation = Quaternion.Euler(0f, 0f, num);
			}
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x00010B1D File Offset: 0x0000ED1D
		public void CloseCard()
		{
			this.Updating = false;
			this.card.SetActive(false);
		}

		// Token: 0x040001F4 RID: 500
		public GameObject card;
	}
}
