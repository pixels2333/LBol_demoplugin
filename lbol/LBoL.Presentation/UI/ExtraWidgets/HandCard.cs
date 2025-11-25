using System;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	public class HandCard : MonoBehaviour
	{
		public CanvasGroup CanvasGroup
		{
			get
			{
				return this.CardWidget.CanvasGroup;
			}
		}
		public bool IsActiveHand
		{
			get
			{
				return this._isActiveHand;
			}
			set
			{
				this._isActiveHand = value;
				if (value)
				{
					AudioManager.Card(1);
					this.CanvasGroup.blocksRaycasts = false;
					this.MoveToParent(this.ActiveHandParent, true);
					return;
				}
				AudioManager.Card(2);
				this.CanvasGroup.blocksRaycasts = true;
				this.MoveToParent(this._isHovered ? this.HoveredParent : this.NormalParent, true);
				this.CostMoreLeft = this.ShouldShowLeftCost;
			}
		}
		public Transform NormalParent { get; set; }
		public Transform HoveredParent { get; set; }
		public Transform ActiveHandParent { get; set; }
		public Vector3 NormalPosition { get; set; }
		public Quaternion NormalRotation { get; set; }
		public Vector3 HoveredPosition { get; set; }
		public Quaternion HoveredRotation { get; set; }
		public Vector3 ActiveHandPosition { get; set; }
		public Quaternion ActiveHandRotation { get; set; }
		public Vector3 SpecialReactingPosition { get; set; }
		public Quaternion SpecialReactingRotation { get; set; }
		public int HandIndex { get; set; }
		public bool ShowShortcut
		{
			get
			{
				return this._showShortcut;
			}
			set
			{
				this._showShortcut = value;
				this.shortcutText.gameObject.SetActive(value);
				this.shortcutText.text = UiManager.GetHandShortcutDisplayString(this.HandIndex);
			}
		}
		public bool IsMostRightHand
		{
			get
			{
				Card card = this.CardWidget.Card;
				return card != null && card.IsMostRightHand;
			}
		}
		public bool ShouldShowLeftCost
		{
			get
			{
				return GameMaster.CostMoreLeft && !this.IsMostRightHand && !this.IsActiveHand;
			}
		}
		public bool CostMoreLeft
		{
			get
			{
				return this.CardWidget.CostMoreLeft;
			}
			set
			{
				this.CardWidget.CostMoreLeft = value;
			}
		}
		public Vector3 PendingUsePosition { get; set; }
		public Vector3 TargetWorldPosition
		{
			get
			{
				Transform parent = base.transform.parent;
				Vector3 vector;
				if (this._pendingUse)
				{
					vector = this.PendingUsePosition;
				}
				else if (this._specialReacting)
				{
					vector = this.SpecialReactingPosition;
				}
				else
				{
					vector = (this._isActiveHand ? this.ActiveHandPosition : (this._isHovered ? this.HoveredPosition : this.NormalPosition));
				}
				return parent.TransformPoint(vector);
			}
		}
		public void SetToInactiveWithoutAudio()
		{
			if (this._isActiveHand)
			{
				this._isActiveHand = false;
				this.CanvasGroup.blocksRaycasts = true;
				this.MoveToParent(this._isHovered ? this.HoveredParent : this.NormalParent, true);
			}
		}
		public bool SpecialReacting
		{
			get
			{
				return this._specialReacting;
			}
			set
			{
				this._specialReacting = value;
			}
		}
		public bool PendingUse
		{
			get
			{
				return this._pendingUse;
			}
			set
			{
				this._pendingUse = value;
				this.CardWidget.ShowManaHand = !this._pendingUse;
			}
		}
		public bool IsDisappearing { get; set; }
		public Card Card
		{
			get
			{
				return this.CardWidget.Card;
			}
		}
		public CardWidget CardWidget { get; set; }
		private void Awake()
		{
			this.ShowShortcut = GameMaster.ShowShortcut;
		}
		public void RefreshStatus()
		{
			this.CardWidget.RefreshStatus();
			if (this.ShowShortcut)
			{
				this.shortcutText.text = UiManager.GetHandShortcutDisplayString(this.HandIndex);
			}
			this.CardWidget.CostMoreLeft = this.ShouldShowLeftCost;
		}
		private void Update()
		{
			if (this.IsDisappearing)
			{
				return;
			}
			this._accumulatedTime += Time.unscaledDeltaTime;
			if (this._specialReacting)
			{
				this.Approach(this.SpecialReactingPosition, this.SpecialReactingRotation, 1.2f, false);
				return;
			}
			if (this._pendingUse)
			{
				this.Approach(this.PendingUsePosition, this.ActiveHandRotation, 1f, false);
				return;
			}
			if (this._isActiveHand)
			{
				this.Approach(this.ActiveHandPosition, this.ActiveHandRotation, 1f, true);
				return;
			}
			if (this._isHovered)
			{
				this.Approach(this.HoveredPosition, this.HoveredRotation, 1.3f, false);
				return;
			}
			this.Approach(this.NormalPosition, this.NormalRotation, 0.7f, false);
		}
		private void OnDestroy()
		{
			if (this.placeHolder != null)
			{
				Object.Destroy(this.placeHolder.gameObject);
			}
		}
		private void MoveToParent(Transform parent, bool worldPositionStays = true)
		{
			if (!parent)
			{
				throw new ArgumentNullException("parent");
			}
			if (this.rectTransform.parent == parent)
			{
				return;
			}
			if (parent == this.NormalParent)
			{
				this.rectTransform.SetParent(parent, worldPositionStays);
				this.rectTransform.SetSiblingIndex(this.placeHolder.GetSiblingIndex());
				this.placeHolder.SetParent(this.rectTransform);
				return;
			}
			if (this.rectTransform.parent == this.NormalParent)
			{
				this.placeHolder.SetParent(this.NormalParent);
				this.placeHolder.SetSiblingIndex(this.rectTransform.GetSiblingIndex());
				this.rectTransform.SetParent(parent, worldPositionStays);
				return;
			}
			this.rectTransform.SetParent(parent, worldPositionStays);
		}
		public void MoveToParentWhenReordering(Transform parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			if (this.placeHolder.parent == this.rectTransform)
			{
				base.transform.SetParent(parent);
				return;
			}
			this.placeHolder.SetParent(parent);
		}
		private Vector2 TiltVector2 { get; set; }
		private Quaternion StandardRotation { get; set; }
		private void ApproachTick(Vector3 position, Quaternion rotation, float scale, bool tilting)
		{
			if (this.rectTransform.localPosition != position)
			{
				this.rectTransform.localPosition = 0.12f.Lerp(this.rectTransform.localPosition, position);
			}
			Vector3 vector = new Vector3(scale, scale, 1f);
			if (this.rectTransform.localScale != vector)
			{
				this.rectTransform.localScale = 0.12f.Lerp(this.rectTransform.localScale, vector);
			}
			if (this.StandardRotation != rotation)
			{
				this.StandardRotation = 0.12f.Lerp(this.StandardRotation, rotation);
			}
			if (tilting)
			{
				Vector2 vector2 = (position - this.rectTransform.localPosition) * 0.001f;
				this.TiltVector2 += vector2;
			}
			if (this.TiltVector2.magnitude > 0.001f)
			{
				if (this.TiltVector2.magnitude > 1f)
				{
					this.TiltVector2 = this.TiltVector2.normalized;
				}
				this.TiltVector2 = 0.05f.Lerp(this.TiltVector2, Vector2.zero);
				Quaternion quaternion = Quaternion.AngleAxis(15f * this.TiltVector2.magnitude, new Vector3(this.TiltVector2.y, -this.TiltVector2.x));
				this.rectTransform.localRotation = quaternion * this.StandardRotation;
				return;
			}
			this.rectTransform.localRotation = this.StandardRotation;
		}
		private void Approach(Vector3 position, Quaternion rotation, float scale, bool tilting = false)
		{
			while (this._tickTime < this._accumulatedTime)
			{
				this._tickTime += 0.005f;
				this.ApproachTick(position, rotation, scale, tilting);
			}
		}
		public bool CanUse { get; private set; }
		public bool CanKickerUse { get; private set; }
		public void SetUsableStatus(bool canUse, bool canKickerUse)
		{
			this.CanUse = canUse;
			this.CanKickerUse = canKickerUse;
			this.UpdateEdge();
		}
		private void UpdateEdge()
		{
			this.CardWidget.SetCardEdge(this.CanUse ? (this.Card.Triggered ? CardWidget.EdgeStatus.High : (this.CanKickerUse ? CardWidget.EdgeStatus.AffordKicker : CardWidget.EdgeStatus.Afford)) : CardWidget.EdgeStatus.None);
		}
		public void StartHover()
		{
			if (this.CardWidget.Card == null)
			{
				throw new InvalidOperationException("PointerEnter for hand without <Card> instance.");
			}
			if (!this._isActiveHand && !this._isHovered)
			{
				this.MoveToParent(this.HoveredParent, true);
			}
			this._isHovered = true;
			this.CostMoreLeft = false;
			this.CardWidget.ShowTooltip();
			AudioManager.Card(0);
		}
		public void EndHover()
		{
			this._isHovered = false;
			if (this.ShouldShowLeftCost)
			{
				this.CostMoreLeft = true;
			}
			if (!this._isActiveHand)
			{
				this.MoveToParent(this.NormalParent, true);
			}
			this.CardWidget.HideTooltip();
		}
		public void CancelUse()
		{
			this.IsActiveHand = false;
			this.UpdateEdge();
			this.EndHover();
		}
		public void ReturnToHand()
		{
			this.SetToInactiveWithoutAudio();
			this.UpdateEdge();
			this.EndHover();
		}
		public void Exile()
		{
			this.ShowShortcut = false;
			this.CardWidget.Exile().RootObject = base.gameObject;
		}
		public void Remove()
		{
			this.ShowShortcut = false;
			this.CardWidget.Remove().RootObject = base.gameObject;
		}
		public void TransformEffect()
		{
			this.CardWidget.PlayTransformEffect();
		}
		public void Summon()
		{
			this.CardWidget.Summon();
		}
		public Transform cardRoot;
		[SerializeField]
		private Transform placeHolder;
		[SerializeField]
		private RectTransform rectTransform;
		[SerializeField]
		private TextMeshProUGUI shortcutText;
		private BattleManaPanel _battleManaPanel;
		private float _tickTime;
		private float _accumulatedTime;
		private bool _isHovered;
		private bool _isActiveHand;
		public const float NormalScale = 0.7f;
		public const float HoverScale = 1.3f;
		public const float ActiveHandScale = 1f;
		public const float SpecialReactingScale = 1.2f;
		private bool _showShortcut;
		private bool _specialReacting;
		private bool _pendingUse;
		private const float ApproachingRatePerTick = 0.12f;
		private const float TiltApproachingRatePerTick = 0.05f;
		private const int TickPerSecond = 200;
		private const float TickDuration = 0.005f;
		private const float MaxAngle = 15f;
		private const float RiseRate = 0.001f;
	}
}
