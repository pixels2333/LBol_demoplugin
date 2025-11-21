using System;
using LBoL.Presentation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x02000086 RID: 134
	public class BackgroundPanel : UiPanel<string>
	{
		// Token: 0x17000127 RID: 295
		// (get) Token: 0x060006B2 RID: 1714 RVA: 0x0001D7F9 File Offset: 0x0001B9F9
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}

		// Token: 0x060006B3 RID: 1715 RVA: 0x0001D7FC File Offset: 0x0001B9FC
		protected override void OnShowing(string bgName)
		{
			this.image.sprite = ResourcesHelper.LoadUiBackground(bgName);
			GameDirector.HideAll();
		}

		// Token: 0x060006B4 RID: 1716 RVA: 0x0001D814 File Offset: 0x0001BA14
		protected override void OnHided()
		{
			Sprite sprite = this.image.sprite;
			this.image.sprite = null;
			if (sprite)
			{
				ResourcesHelper.Release(sprite);
			}
			GameDirector.RevealAll(true);
		}

		// Token: 0x04000449 RID: 1097
		[SerializeField]
		private Image image;
	}
}
