using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000051 RID: 81
	public class DollInfoWidget : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x060004B7 RID: 1207 RVA: 0x000138F3 File Offset: 0x00011AF3
		private void Awake()
		{
			this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060004B8 RID: 1208 RVA: 0x00013910 File Offset: 0x00011B10
		// (set) Token: 0x060004B9 RID: 1209 RVA: 0x00013918 File Offset: 0x00011B18
		public Doll Doll
		{
			get
			{
				return this._doll;
			}
			set
			{
				this._doll = value;
				this.Refresh();
			}
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060004BA RID: 1210 RVA: 0x00013927 File Offset: 0x00011B27
		// (set) Token: 0x060004BB RID: 1211 RVA: 0x00013934 File Offset: 0x00011B34
		public Transform TargetTransform
		{
			get
			{
				return this._scenePositionTier.TargetTransform;
			}
			set
			{
				this._scenePositionTier.TargetTransform = value;
			}
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x00013944 File Offset: 0x00011B44
		public void Refresh()
		{
			if (this._doll == null)
			{
				return;
			}
			int? num = this._doll.UpCounter;
			if (num != null)
			{
				int valueOrDefault = num.GetValueOrDefault();
				this.upCounterText.text = valueOrDefault.ToString();
				this.upCounterText.color = this._doll.UpCounterColor;
			}
			else
			{
				this.upCounterText.text = "";
			}
			num = this._doll.DownCounter;
			if (num != null)
			{
				int valueOrDefault2 = num.GetValueOrDefault();
				this.downCounterText.text = valueOrDefault2.ToString();
				this.downCounterText.color = this._doll.DownCounterColor;
			}
			else
			{
				this.downCounterText.text = "";
			}
			if (!this._doll.HasMagic)
			{
				this.magicIcon.gameObject.SetActive(false);
				this.magicText.gameObject.SetActive(false);
				return;
			}
			int magicIconIndex = this.GetMagicIconIndex(this._doll.Magic, this._doll.MaxMagic);
			Sprite sprite = this.magicSprites.TryGetValue(magicIconIndex);
			if (sprite != null)
			{
				this.magicIcon.gameObject.SetActive(true);
				this.magicIcon.sprite = sprite;
				this.magicText.gameObject.SetActive(false);
				return;
			}
			this.magicIcon.gameObject.SetActive(false);
			this.magicText.gameObject.SetActive(true);
			this.magicText.text = this._doll.Magic.ToString() + "/" + this._doll.MaxMagic.ToString();
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x00013B00 File Offset: 0x00011D00
		private int GetMagicIconIndex(int molecule, int denominator)
		{
			switch (denominator)
			{
			case 1:
				if (molecule == 0)
				{
					return 0;
				}
				if (molecule == 1)
				{
					return 1;
				}
				break;
			case 2:
				switch (molecule)
				{
				case 0:
					return 2;
				case 1:
					return 3;
				case 2:
					return 4;
				}
				break;
			case 3:
				switch (molecule)
				{
				case 0:
					return 5;
				case 1:
					return 6;
				case 2:
					return 7;
				case 3:
					return 8;
				}
				break;
			case 4:
				switch (molecule)
				{
				case 0:
					return 9;
				case 1:
					return 10;
				case 2:
					return 11;
				case 3:
					return 12;
				case 4:
					return 13;
				}
				break;
			}
			return -1;
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x00013B9C File Offset: 0x00011D9C
		public void OnPointerEnter(PointerEventData eventData)
		{
			AudioManager.Button(2);
			if (!this._pendingUse && this.Doll.Usable && (!this.Doll.HasMagic || this.Doll.Magic >= this.Doll.MagicCost))
			{
				TargetType targetType = this.Doll.TargetType;
				if (targetType != TargetType.SingleEnemy)
				{
					foreach (UnitView unitView in GameDirector.EnumeratePotentialTargets(targetType))
					{
						unitView.SelectingVisible = true;
					}
				}
			}
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x00013C38 File Offset: 0x00011E38
		public void OnPointerExit(PointerEventData eventData)
		{
			if (this.Doll.TargetType != TargetType.SingleEnemy)
			{
				foreach (UnitView unitView in GameDirector.EnumeratePotentialTargets(this.Doll.TargetType))
				{
					unitView.SelectingVisible = false;
				}
			}
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x00013C9C File Offset: 0x00011E9C
		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && !UiManager.GetPanel<PlayBoard>().IsTempLockedFromMinimize)
			{
				this.StartUsingDoll(true);
			}
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x00013CB9 File Offset: 0x00011EB9
		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				UiManager.GetPanel<PlayBoard>().OnPointerUp(eventData);
			}
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x00013CCE File Offset: 0x00011ECE
		public void UseDollFromKey()
		{
			this.StartUsingDoll(false);
		}

		// Token: 0x060004C3 RID: 1219 RVA: 0x00013CD8 File Offset: 0x00011ED8
		private void StartUsingDoll(bool fromClick)
		{
			if (this._pendingUse || !this.Doll.Usable || !this.Doll.Owner.IsInTurn)
			{
				return;
			}
			if (this.Doll.HasMagic && this.Doll.Magic < this.Doll.MagicCost)
			{
				UiManager.GetPanel<PlayBoard>().ShowLowMagic();
				AudioManager.PlayUi("UltCantUse", false);
				return;
			}
			AudioManager.PlayUi("UltClick", false);
			this._pendingUse = true;
			if (this.Doll.TargetType == TargetType.SingleEnemy)
			{
				UiManager.GetPanel<PlayBoard>().EnableSelector(this.Doll, base.transform.position, fromClick);
				return;
			}
			UnitSelector unitSelector;
			switch (this.Doll.TargetType)
			{
			case TargetType.Nobody:
				unitSelector = UnitSelector.Nobody;
				goto IL_00FA;
			case TargetType.AllEnemies:
				unitSelector = UnitSelector.AllEnemies;
				goto IL_00FA;
			case TargetType.RandomEnemy:
				unitSelector = UnitSelector.RandomEnemy;
				goto IL_00FA;
			case TargetType.Self:
				unitSelector = UnitSelector.Self;
				goto IL_00FA;
			case TargetType.All:
				unitSelector = UnitSelector.All;
				goto IL_00FA;
			}
			throw new ArgumentOutOfRangeException();
			IL_00FA:
			UnitSelector unitSelector2 = unitSelector;
			UiManager.GetPanel<PlayBoard>().RequestUseDoll(this.Doll, unitSelector2);
			foreach (UnitView unitView in GameDirector.EnumeratePotentialTargets(this.Doll.TargetType))
			{
				unitView.SelectingVisible = false;
			}
		}

		// Token: 0x060004C4 RID: 1220 RVA: 0x00013E3C File Offset: 0x0001203C
		public void CancelUse()
		{
			this._pendingUse = false;
		}

		// Token: 0x060004C5 RID: 1221 RVA: 0x00013E45 File Offset: 0x00012045
		public void ConsumeMagic()
		{
			this._pendingUse = false;
		}

		// Token: 0x04000288 RID: 648
		[SerializeField]
		private Image magicIcon;

		// Token: 0x04000289 RID: 649
		[SerializeField]
		private TextMeshProUGUI magicText;

		// Token: 0x0400028A RID: 650
		[SerializeField]
		private TextMeshProUGUI upCounterText;

		// Token: 0x0400028B RID: 651
		[SerializeField]
		private TextMeshProUGUI downCounterText;

		// Token: 0x0400028C RID: 652
		[SerializeField]
		private List<Sprite> magicSprites;

		// Token: 0x0400028D RID: 653
		private ScenePositionTier _scenePositionTier;

		// Token: 0x0400028E RID: 654
		private Doll _doll;

		// Token: 0x0400028F RID: 655
		private bool _pendingUse;
	}
}
