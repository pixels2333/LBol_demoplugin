using System;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000CC RID: 204
	public class HandCard : MonoBehaviour
	{
		// Token: 0x170001DE RID: 478
		// (get) Token: 0x06000C36 RID: 3126 RVA: 0x0003EB64 File Offset: 0x0003CD64
		public CanvasGroup CanvasGroup
		{
			get
			{
				return this.CardWidget.CanvasGroup;
			}
		}

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06000C37 RID: 3127 RVA: 0x0003EB71 File Offset: 0x0003CD71
		// (set) Token: 0x06000C38 RID: 3128 RVA: 0x0003EB7C File Offset: 0x0003CD7C
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

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06000C39 RID: 3129 RVA: 0x0003EBEE File Offset: 0x0003CDEE
		// (set) Token: 0x06000C3A RID: 3130 RVA: 0x0003EBF6 File Offset: 0x0003CDF6
		public Transform NormalParent { get; set; }

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000C3B RID: 3131 RVA: 0x0003EBFF File Offset: 0x0003CDFF
		// (set) Token: 0x06000C3C RID: 3132 RVA: 0x0003EC07 File Offset: 0x0003CE07
		public Transform HoveredParent { get; set; }

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06000C3D RID: 3133 RVA: 0x0003EC10 File Offset: 0x0003CE10
		// (set) Token: 0x06000C3E RID: 3134 RVA: 0x0003EC18 File Offset: 0x0003CE18
		public Transform ActiveHandParent { get; set; }

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06000C3F RID: 3135 RVA: 0x0003EC21 File Offset: 0x0003CE21
		// (set) Token: 0x06000C40 RID: 3136 RVA: 0x0003EC29 File Offset: 0x0003CE29
		public Vector3 NormalPosition { get; set; }

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000C41 RID: 3137 RVA: 0x0003EC32 File Offset: 0x0003CE32
		// (set) Token: 0x06000C42 RID: 3138 RVA: 0x0003EC3A File Offset: 0x0003CE3A
		public Quaternion NormalRotation { get; set; }

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06000C43 RID: 3139 RVA: 0x0003EC43 File Offset: 0x0003CE43
		// (set) Token: 0x06000C44 RID: 3140 RVA: 0x0003EC4B File Offset: 0x0003CE4B
		public Vector3 HoveredPosition { get; set; }

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x06000C45 RID: 3141 RVA: 0x0003EC54 File Offset: 0x0003CE54
		// (set) Token: 0x06000C46 RID: 3142 RVA: 0x0003EC5C File Offset: 0x0003CE5C
		public Quaternion HoveredRotation { get; set; }

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x06000C47 RID: 3143 RVA: 0x0003EC65 File Offset: 0x0003CE65
		// (set) Token: 0x06000C48 RID: 3144 RVA: 0x0003EC6D File Offset: 0x0003CE6D
		public Vector3 ActiveHandPosition { get; set; }

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06000C49 RID: 3145 RVA: 0x0003EC76 File Offset: 0x0003CE76
		// (set) Token: 0x06000C4A RID: 3146 RVA: 0x0003EC7E File Offset: 0x0003CE7E
		public Quaternion ActiveHandRotation { get; set; }

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000C4B RID: 3147 RVA: 0x0003EC87 File Offset: 0x0003CE87
		// (set) Token: 0x06000C4C RID: 3148 RVA: 0x0003EC8F File Offset: 0x0003CE8F
		public Vector3 SpecialReactingPosition { get; set; }

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000C4D RID: 3149 RVA: 0x0003EC98 File Offset: 0x0003CE98
		// (set) Token: 0x06000C4E RID: 3150 RVA: 0x0003ECA0 File Offset: 0x0003CEA0
		public Quaternion SpecialReactingRotation { get; set; }

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000C4F RID: 3151 RVA: 0x0003ECA9 File Offset: 0x0003CEA9
		// (set) Token: 0x06000C50 RID: 3152 RVA: 0x0003ECB1 File Offset: 0x0003CEB1
		public int HandIndex { get; set; }

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06000C51 RID: 3153 RVA: 0x0003ECBA File Offset: 0x0003CEBA
		// (set) Token: 0x06000C52 RID: 3154 RVA: 0x0003ECC2 File Offset: 0x0003CEC2
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

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x06000C53 RID: 3155 RVA: 0x0003ECF4 File Offset: 0x0003CEF4
		public bool IsMostRightHand
		{
			get
			{
				Card card = this.CardWidget.Card;
				return card != null && card.IsMostRightHand;
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x06000C54 RID: 3156 RVA: 0x0003ED18 File Offset: 0x0003CF18
		public bool ShouldShowLeftCost
		{
			get
			{
				return GameMaster.CostMoreLeft && !this.IsMostRightHand && !this.IsActiveHand;
			}
		}

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06000C55 RID: 3157 RVA: 0x0003ED34 File Offset: 0x0003CF34
		// (set) Token: 0x06000C56 RID: 3158 RVA: 0x0003ED41 File Offset: 0x0003CF41
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

		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x06000C57 RID: 3159 RVA: 0x0003ED4F File Offset: 0x0003CF4F
		// (set) Token: 0x06000C58 RID: 3160 RVA: 0x0003ED57 File Offset: 0x0003CF57
		public Vector3 PendingUsePosition { get; set; }

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x06000C59 RID: 3161 RVA: 0x0003ED60 File Offset: 0x0003CF60
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

		// Token: 0x06000C5A RID: 3162 RVA: 0x0003EDC7 File Offset: 0x0003CFC7
		public void SetToInactiveWithoutAudio()
		{
			if (this._isActiveHand)
			{
				this._isActiveHand = false;
				this.CanvasGroup.blocksRaycasts = true;
				this.MoveToParent(this._isHovered ? this.HoveredParent : this.NormalParent, true);
			}
		}

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06000C5B RID: 3163 RVA: 0x0003EE01 File Offset: 0x0003D001
		// (set) Token: 0x06000C5C RID: 3164 RVA: 0x0003EE09 File Offset: 0x0003D009
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

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000C5D RID: 3165 RVA: 0x0003EE12 File Offset: 0x0003D012
		// (set) Token: 0x06000C5E RID: 3166 RVA: 0x0003EE1A File Offset: 0x0003D01A
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

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06000C5F RID: 3167 RVA: 0x0003EE37 File Offset: 0x0003D037
		// (set) Token: 0x06000C60 RID: 3168 RVA: 0x0003EE3F File Offset: 0x0003D03F
		public bool IsDisappearing { get; set; }

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06000C61 RID: 3169 RVA: 0x0003EE48 File Offset: 0x0003D048
		public Card Card
		{
			get
			{
				return this.CardWidget.Card;
			}
		}

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06000C62 RID: 3170 RVA: 0x0003EE55 File Offset: 0x0003D055
		// (set) Token: 0x06000C63 RID: 3171 RVA: 0x0003EE5D File Offset: 0x0003D05D
		public CardWidget CardWidget { get; set; }

		// Token: 0x06000C64 RID: 3172 RVA: 0x0003EE66 File Offset: 0x0003D066
		private void Awake()
		{
			this.ShowShortcut = GameMaster.ShowShortcut;
		}

		// Token: 0x06000C65 RID: 3173 RVA: 0x0003EE73 File Offset: 0x0003D073
		public void RefreshStatus()
		{
			this.CardWidget.RefreshStatus();
			if (this.ShowShortcut)
			{
				this.shortcutText.text = UiManager.GetHandShortcutDisplayString(this.HandIndex);
			}
			this.CardWidget.CostMoreLeft = this.ShouldShowLeftCost;
		}

		// Token: 0x06000C66 RID: 3174 RVA: 0x0003EEB0 File Offset: 0x0003D0B0
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

		// Token: 0x06000C67 RID: 3175 RVA: 0x0003EF74 File Offset: 0x0003D174
		private void OnDestroy()
		{
			if (this.placeHolder != null)
			{
				Object.Destroy(this.placeHolder.gameObject);
			}
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x0003EF94 File Offset: 0x0003D194
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

		// Token: 0x06000C69 RID: 3177 RVA: 0x0003F068 File Offset: 0x0003D268
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

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x06000C6A RID: 3178 RVA: 0x0003F0BA File Offset: 0x0003D2BA
		// (set) Token: 0x06000C6B RID: 3179 RVA: 0x0003F0C2 File Offset: 0x0003D2C2
		private Vector2 TiltVector2 { get; set; }

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06000C6C RID: 3180 RVA: 0x0003F0CB File Offset: 0x0003D2CB
		// (set) Token: 0x06000C6D RID: 3181 RVA: 0x0003F0D3 File Offset: 0x0003D2D3
		private Quaternion StandardRotation { get; set; }

		// Token: 0x06000C6E RID: 3182 RVA: 0x0003F0DC File Offset: 0x0003D2DC
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

		// Token: 0x06000C6F RID: 3183 RVA: 0x0003F276 File Offset: 0x0003D476
		private void Approach(Vector3 position, Quaternion rotation, float scale, bool tilting = false)
		{
			while (this._tickTime < this._accumulatedTime)
			{
				this._tickTime += 0.005f;
				this.ApproachTick(position, rotation, scale, tilting);
			}
		}

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06000C70 RID: 3184 RVA: 0x0003F2A5 File Offset: 0x0003D4A5
		// (set) Token: 0x06000C71 RID: 3185 RVA: 0x0003F2AD File Offset: 0x0003D4AD
		public bool CanUse { get; private set; }

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06000C72 RID: 3186 RVA: 0x0003F2B6 File Offset: 0x0003D4B6
		// (set) Token: 0x06000C73 RID: 3187 RVA: 0x0003F2BE File Offset: 0x0003D4BE
		public bool CanKickerUse { get; private set; }

		// Token: 0x06000C74 RID: 3188 RVA: 0x0003F2C7 File Offset: 0x0003D4C7
		public void SetUsableStatus(bool canUse, bool canKickerUse)
		{
			this.CanUse = canUse;
			this.CanKickerUse = canKickerUse;
			this.UpdateEdge();
		}

		// Token: 0x06000C75 RID: 3189 RVA: 0x0003F2DD File Offset: 0x0003D4DD
		private void UpdateEdge()
		{
			this.CardWidget.SetCardEdge(this.CanUse ? (this.Card.Triggered ? CardWidget.EdgeStatus.High : (this.CanKickerUse ? CardWidget.EdgeStatus.AffordKicker : CardWidget.EdgeStatus.Afford)) : CardWidget.EdgeStatus.None);
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x0003F314 File Offset: 0x0003D514
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

		// Token: 0x06000C77 RID: 3191 RVA: 0x0003F375 File Offset: 0x0003D575
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

		// Token: 0x06000C78 RID: 3192 RVA: 0x0003F3AD File Offset: 0x0003D5AD
		public void CancelUse()
		{
			this.IsActiveHand = false;
			this.UpdateEdge();
			this.EndHover();
		}

		// Token: 0x06000C79 RID: 3193 RVA: 0x0003F3C2 File Offset: 0x0003D5C2
		public void ReturnToHand()
		{
			this.SetToInactiveWithoutAudio();
			this.UpdateEdge();
			this.EndHover();
		}

		// Token: 0x06000C7A RID: 3194 RVA: 0x0003F3D6 File Offset: 0x0003D5D6
		public void Exile()
		{
			this.ShowShortcut = false;
			this.CardWidget.Exile().RootObject = base.gameObject;
		}

		// Token: 0x06000C7B RID: 3195 RVA: 0x0003F3F5 File Offset: 0x0003D5F5
		public void Remove()
		{
			this.ShowShortcut = false;
			this.CardWidget.Remove().RootObject = base.gameObject;
		}

		// Token: 0x06000C7C RID: 3196 RVA: 0x0003F414 File Offset: 0x0003D614
		public void TransformEffect()
		{
			this.CardWidget.PlayTransformEffect();
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x0003F421 File Offset: 0x0003D621
		public void Summon()
		{
			this.CardWidget.Summon();
		}

		// Token: 0x04000970 RID: 2416
		public Transform cardRoot;

		// Token: 0x04000971 RID: 2417
		[SerializeField]
		private Transform placeHolder;

		// Token: 0x04000972 RID: 2418
		[SerializeField]
		private RectTransform rectTransform;

		// Token: 0x04000973 RID: 2419
		[SerializeField]
		private TextMeshProUGUI shortcutText;

		// Token: 0x04000974 RID: 2420
		private BattleManaPanel _battleManaPanel;

		// Token: 0x04000975 RID: 2421
		private float _tickTime;

		// Token: 0x04000976 RID: 2422
		private float _accumulatedTime;

		// Token: 0x04000977 RID: 2423
		private bool _isHovered;

		// Token: 0x04000978 RID: 2424
		private bool _isActiveHand;

		// Token: 0x04000984 RID: 2436
		public const float NormalScale = 0.7f;

		// Token: 0x04000985 RID: 2437
		public const float HoverScale = 1.3f;

		// Token: 0x04000986 RID: 2438
		public const float ActiveHandScale = 1f;

		// Token: 0x04000987 RID: 2439
		public const float SpecialReactingScale = 1.2f;

		// Token: 0x04000989 RID: 2441
		private bool _showShortcut;

		// Token: 0x0400098B RID: 2443
		private bool _specialReacting;

		// Token: 0x0400098C RID: 2444
		private bool _pendingUse;

		// Token: 0x0400098F RID: 2447
		private const float ApproachingRatePerTick = 0.12f;

		// Token: 0x04000990 RID: 2448
		private const float TiltApproachingRatePerTick = 0.05f;

		// Token: 0x04000991 RID: 2449
		private const int TickPerSecond = 200;

		// Token: 0x04000992 RID: 2450
		private const float TickDuration = 0.005f;

		// Token: 0x04000993 RID: 2451
		private const float MaxAngle = 15f;

		// Token: 0x04000994 RID: 2452
		private const float RiseRate = 0.001f;
	}
}
