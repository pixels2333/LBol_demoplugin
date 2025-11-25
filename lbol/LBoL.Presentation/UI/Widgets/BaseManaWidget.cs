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
	public class BaseManaWidget : MonoBehaviour
	{
		public ManaColor ManaColor { get; private set; }
		public int BattleManaAmount { get; private set; }
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
		private void LockPresentation()
		{
			this.baseManaImage.DOFade(0f, 0.2f).SetAutoKill(true).SetUpdate(true);
			this.baseManaLockImage.DOFade(1f, 0.2f).SetAutoKill(true).SetUpdate(true);
		}
		private void UnlockPresentation()
		{
			this.baseManaImage.DOFade(1f, 0.2f).SetAutoKill(true).SetUpdate(true);
			this.baseManaLockImage.DOFade(0f, 0.2f).SetAutoKill(true).SetUpdate(true);
		}
		public Vector3 CenterWorldPosition
		{
			get
			{
				RectTransform rectTransform = (RectTransform)base.transform;
				Vector2 center = rectTransform.rect.center;
				return rectTransform.TransformPoint(center);
			}
		}
		public void SetBaseMana(ManaColor color)
		{
			this.baseManaGroup.SetActive(true);
			this.battleManaGroup.SetActive(false);
			this.ManaColor = color;
			this.baseManaImage.sprite = CollectionExtensions.GetValueOrDefault<ManaColor, Sprite>(this.baseManaSpriteTable, color);
			this.baseManaLockImage.sprite = CollectionExtensions.GetValueOrDefault<ManaColor, Sprite>(this.lockTable, color);
		}
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
		[SerializeField]
		private GameObject baseManaGroup;
		[SerializeField]
		private GameObject battleManaGroup;
		[SerializeField]
		private Image baseManaImage;
		[SerializeField]
		private Image baseManaLockImage;
		[SerializeField]
		private Image battleManaImage;
		[SerializeField]
		private TextMeshProUGUI battleManaAmountTmp;
		[SerializeField]
		private AssociationList<ManaColor, Sprite> baseManaSpriteTable;
		[SerializeField]
		private AssociationList<ManaColor, Sprite> lockTable;
		[SerializeField]
		private AssociationList<ManaColor, Sprite> battleManaTable;
		private bool _isLocked;
	}
}
