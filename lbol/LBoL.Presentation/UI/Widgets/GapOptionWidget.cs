using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core.GapOptions;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200005A RID: 90
	public class GapOptionWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Token: 0x170000DA RID: 218
		// (get) Token: 0x0600050F RID: 1295 RVA: 0x00015A98 File Offset: 0x00013C98
		// (set) Token: 0x06000510 RID: 1296 RVA: 0x00015AA0 File Offset: 0x00013CA0
		public GapOptionsPanel Parent { get; set; }

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000511 RID: 1297 RVA: 0x00015AA9 File Offset: 0x00013CA9
		// (set) Token: 0x06000512 RID: 1298 RVA: 0x00015AB1 File Offset: 0x00013CB1
		public bool Active { get; set; } = true;

		// Token: 0x06000513 RID: 1299 RVA: 0x00015ABA File Offset: 0x00013CBA
		private void Awake()
		{
			this.optionTip.gameObject.SetActive(false);
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x00015ACD File Offset: 0x00013CCD
		public void OnLocalizeChanged()
		{
			if (this._option != null)
			{
				this.optionName.SetText(this._option.Name, true);
				this.optionTipText.SetText(this._option.Description, true);
			}
		}

		// Token: 0x06000515 RID: 1301 RVA: 0x00015B05 File Offset: 0x00013D05
		public void SetOption(GapOption option, Sprite sprite)
		{
			this._option = option;
			this.optionName.text = option.Name;
			this.image.sprite = sprite;
			this.optionTipText.text = option.Description;
		}

		// Token: 0x06000516 RID: 1302 RVA: 0x00015B3C File Offset: 0x00013D3C
		public void OnPointerClick(PointerEventData eventData)
		{
			if (this.Active)
			{
				this.Parent.OptionClicked(this._option);
			}
		}

		// Token: 0x06000517 RID: 1303 RVA: 0x00015B58 File Offset: 0x00013D58
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.root.DOScale(1.1f, 0.2f).SetLink(base.gameObject);
			this.Parent.StartHoverOption(this._option);
			this.optionTip.gameObject.SetActive(true);
		}

		// Token: 0x06000518 RID: 1304 RVA: 0x00015BA8 File Offset: 0x00013DA8
		public void OnPointerExit(PointerEventData eventData)
		{
			this.root.DOScale(1f, 0.2f).SetLink(base.gameObject);
			this.Parent.EndHoverOption();
			this.optionTip.gameObject.SetActive(false);
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x00015BE7 File Offset: 0x00013DE7
		public void OnDisable()
		{
			this.optionTip.gameObject.SetActive(false);
		}

		// Token: 0x040002D2 RID: 722
		[SerializeField]
		private Transform root;

		// Token: 0x040002D3 RID: 723
		[SerializeField]
		private Image image;

		// Token: 0x040002D4 RID: 724
		[SerializeField]
		private TextMeshProUGUI optionName;

		// Token: 0x040002D5 RID: 725
		[SerializeField]
		private Transform optionTip;

		// Token: 0x040002D6 RID: 726
		[SerializeField]
		private TextMeshProUGUI optionTipText;

		// Token: 0x040002D9 RID: 729
		private GapOption _option;
	}
}
