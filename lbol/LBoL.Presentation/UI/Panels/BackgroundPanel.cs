using System;
using LBoL.Presentation.Units;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class BackgroundPanel : UiPanel<string>
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}
		protected override void OnShowing(string bgName)
		{
			this.image.sprite = ResourcesHelper.LoadUiBackground(bgName);
			GameDirector.HideAll();
		}
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
		[SerializeField]
		private Image image;
	}
}
