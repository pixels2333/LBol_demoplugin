using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.EntityLib.Adventures.Shared23;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class NazrinDetectPanel : UiAdventurePanel<NazrinDetect>
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		private void Update()
		{
			if (this.currentIndex != (int)this.pointer.eulerAngles.z / 60)
			{
				int num = this.currentIndex;
				this.currentIndex = (int)this.pointer.eulerAngles.z / 60;
				this.cursor.eulerAngles = new Vector3(0f, 0f, (float)(30 + this.currentIndex * 60));
				this.iconList[this.currentIndex].transform.DOScale(1.1f, 0.1f).From(1f, true, false);
				this.iconList[num].transform.DOScale(1f, 0.1f).From(1.1f, true, false);
			}
		}
		public void StartPlate(int finalIndex)
		{
		}
		[RuntimeCommand("showWheel", "")]
		[UsedImplicitly]
		public IEnumerator ShowWheel()
		{
			this.pointer.transform.eulerAngles = Vector3.zero;
			base.Show();
			this.panel.DOFade(1f, 0.2f).From(0f, true, false);
			this.panel.transform.DOScale(1f, 0.5f).From(0.1f, true, false);
			yield return new WaitForSeconds(0.5f);
			yield break;
		}
		[RuntimeCommand("hideWheel", "")]
		[UsedImplicitly]
		public IEnumerator HideWheel()
		{
			this.panel.DOFade(0f, 0.5f).From(1f, true, false);
			yield return new WaitForSeconds(0.5f);
			base.Hide();
			yield return new WaitForSeconds(0.5f);
			yield break;
		}
		[RuntimeCommand("roll", "")]
		[UsedImplicitly]
		public IEnumerator Roll(int resultIndex)
		{
			this.pointer.transform.DORotate(new Vector3(0f, 0f, (float)(3630 + resultIndex * 60)), 4f, RotateMode.FastBeyond360).From(Vector3.zero, true, false);
			yield return new WaitForSeconds(5f);
			yield break;
		}
		[SerializeField]
		private CanvasGroup panel;
		[SerializeField]
		private Transform pointer;
		[SerializeField]
		private Transform cursor;
		[SerializeField]
		private List<Image> iconList = new List<Image>();
		private int currentIndex;
	}
}
