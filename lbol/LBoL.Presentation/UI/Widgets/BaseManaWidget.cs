using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200003C RID: 60
	public class BaseManaWidget : MonoBehaviour
	{
		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060003E4 RID: 996 RVA: 0x0000FFE7 File Offset: 0x0000E1E7
		// (set) Token: 0x060003E5 RID: 997 RVA: 0x0000FFEF File Offset: 0x0000E1EF
		public ManaColor ManaColor { get; private set; }

		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x060003E6 RID: 998 RVA: 0x0000FFF8 File Offset: 0x0000E1F8
		// (set) Token: 0x060003E7 RID: 999 RVA: 0x00010000 File Offset: 0x0000E200
		public int BattleManaAmount { get; private set; }

		// Token: 0x170000A1 RID: 161
		// (get) Token: 0x060003E8 RID: 1000 RVA: 0x00010009 File Offset: 0x0000E209
		// (set) Token: 0x060003E9 RID: 1001 RVA: 0x00010014 File Offset: 0x0000E214
		public bool IsLocked
		{
			get
			{
				return this._isLocked;
			}
			set
			{
				if (this._isLocked != value)
				{
					this._isLocked = value;
					this.baseManaLockImage.DOKill(false);
					if (value)
					{
						this.LockPresentation();
					}
					else
					{
						this.UnlockPresentation();
					}
					this.baseManaLockImage.DOFade(value ? 0.5f : 0f, 0.2f).SetAutoKill(true).SetUpdate(true);
				}
			}
		}

		// Token: 0x060003EA RID: 1002 RVA: 0x0001007C File Offset: 0x0000E27C
		private void LockPresentation()
		{
			this.baseManaImage.DOFade(0f, 0.2f).SetAutoKill(true).SetUpdate(true);
			this.baseManaLockImage.DOFade(1f, 0.2f).SetAutoKill(true).SetUpdate(true);
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x000100D0 File Offset: 0x0000E2D0
		private void UnlockPresentation()
		{
			this.baseManaImage.DOFade(1f, 0.2f).SetAutoKill(true).SetUpdate(true);
			this.baseManaLockImage.DOFade(0f, 0.2f).SetAutoKill(true).SetUpdate(true);
		}

		// Token: 0x170000A2 RID: 162
		// (get) Token: 0x060003EC RID: 1004 RVA: 0x00010124 File Offset: 0x0000E324
		public Vector3 CenterWorldPosition
		{
			get
			{
				RectTransform rectTransform = (RectTransform)base.transform;
				Vector2 center = rectTransform.rect.center;
				return rectTransform.TransformPoint(center);
			}
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x00010158 File Offset: 0x0000E358
		public void SetBaseMana(ManaColor color)
		{
			this.baseManaGroup.SetActive(true);
			this.battleManaGroup.SetActive(false);
			this.ManaColor = color;
			this.baseManaImage.sprite = CollectionExtensions.GetValueOrDefault<ManaColor, Sprite>(this.baseManaSpriteTable, color);
			this.baseManaLockImage.sprite = CollectionExtensions.GetValueOrDefault<ManaColor, Sprite>(this.lockTable, color);
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x000101B4 File Offset: 0x0000E3B4
		public void SetBattleMana(ManaColor color, int amount)
		{
			this.baseManaGroup.SetActive(false);
			this.battleManaGroup.SetActive(true);
			this.ManaColor = color;
			this.BattleManaAmount = amount;
			this.battleManaImage.sprite = CollectionExtensions.GetValueOrDefault<ManaColor, Sprite>(this.battleManaTable, color);
			this.battleManaAmountTmp.text = amount.ToString();
			this.battleManaAmountTmp.gameObject.SetActive(amount > 1);
		}

		// Token: 0x040001C9 RID: 457
		[SerializeField]
		private GameObject baseManaGroup;

		// Token: 0x040001CA RID: 458
		[SerializeField]
		private GameObject battleManaGroup;

		// Token: 0x040001CB RID: 459
		[SerializeField]
		private Image baseManaImage;

		// Token: 0x040001CC RID: 460
		[SerializeField]
		private Image baseManaLockImage;

		// Token: 0x040001CD RID: 461
		[SerializeField]
		private Image battleManaImage;

		// Token: 0x040001CE RID: 462
		[SerializeField]
		private TextMeshProUGUI battleManaAmountTmp;

		// Token: 0x040001CF RID: 463
		[SerializeField]
		private AssociationList<ManaColor, Sprite> baseManaSpriteTable;

		// Token: 0x040001D0 RID: 464
		[SerializeField]
		private AssociationList<ManaColor, Sprite> lockTable;

		// Token: 0x040001D1 RID: 465
		[SerializeField]
		private AssociationList<ManaColor, Sprite> battleManaTable;

		// Token: 0x040001D4 RID: 468
		private bool _isLocked;
	}
}
