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
	public class DollInfoWidget : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		private void Awake()
		{
			this._scenePositionTier = base.GetComponent<ScenePositionTier>() ?? base.gameObject.AddComponent<ScenePositionTier>();
		}
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
		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left && !UiManager.GetPanel<PlayBoard>().IsTempLockedFromMinimize)
			{
				this.StartUsingDoll(true);
			}
		}
		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				UiManager.GetPanel<PlayBoard>().OnPointerUp(eventData);
			}
		}
		public void UseDollFromKey()
		{
			this.StartUsingDoll(false);
		}
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
		public void CancelUse()
		{
			this._pendingUse = false;
		}
		public void ConsumeMagic()
		{
			this._pendingUse = false;
		}
		[SerializeField]
		private Image magicIcon;
		[SerializeField]
		private TextMeshProUGUI magicText;
		[SerializeField]
		private TextMeshProUGUI upCounterText;
		[SerializeField]
		private TextMeshProUGUI downCounterText;
		[SerializeField]
		private List<Sprite> magicSprites;
		private ScenePositionTier _scenePositionTier;
		private Doll _doll;
		private bool _pendingUse;
	}
}
