using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.Presentation.Effect;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000A7 RID: 167
	public sealed class PlayBoard : UiPanel, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060008ED RID: 2285 RVA: 0x0002D5F4 File Offset: 0x0002B7F4
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}

		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060008EE RID: 2286 RVA: 0x0002D5F7 File Offset: 0x0002B7F7
		public CardUi CardUi
		{
			get
			{
				return this.cardUi;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060008EF RID: 2287 RVA: 0x0002D5FF File Offset: 0x0002B7FF
		public Transform ActiveHandParent
		{
			get
			{
				return this.targetSelector.transform;
			}
		}

		// Token: 0x060008F0 RID: 2288 RVA: 0x0002D60C File Offset: 0x0002B80C
		public void Awake()
		{
			this.endTurnButton.onClick.AddListener(new UnityAction(this.UI_EndTurn));
			SimpleTooltipSource.CreateWithTooltipKeyAndArgs(this.endTurnButton.gameObject, "EndTurn", new object[] { 5 }).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.endTurnEdgeCg.alpha = 0f;
			this.endTurnButton.gameObject.SetActive(false);
			this.InitEffects();
		}

		// Token: 0x060008F1 RID: 2289 RVA: 0x0002D688 File Offset: 0x0002B888
		public override void OnLocaleChanged()
		{
			this._moneyNotEnough = "ErrorChat.MoneyNotEnough".Localize(true);
			this._lowMana = "ErrorChat.LowMana".Localize(true);
			this._lowPower = "ErrorChat.LowPower".Localize(true);
			this._lowMagic = "ErrorChat.LowMagic".Localize(true);
			this._lowLoyalty = "ErrorChat.LowLoyalty".Localize(true);
			this._cardNotInHand = "ErrorChat.CardNotInHand".Localize(true);
			this._cardCostChanged = "ErrorChat.CardCostChanged".Localize(true);
			this._usUsedThisBattle = "ErrorChat.UsUsedThisBattle".Localize(true);
			this._usUsedThisTurn = "ErrorChat.UsUsedThisTurn".Localize(true);
			this._targetAlreadyDead = "ErrorChat.TargetAlreadyDead".Localize(true);
			this._handFull = "ErrorChat.HandFull".Localize(true);
			this._emptyDraw = "ErrorChat.EmptyDraw".Localize(true);
		}

		// Token: 0x060008F2 RID: 2290 RVA: 0x0002D764 File Offset: 0x0002B964
		private void Update()
		{
			if (!this._pointered || this._status == PlayBoard.InteractionStatus.Inactive)
			{
				return;
			}
			if (Mouse.current == null)
			{
				return;
			}
			float num;
			float num2;
			Mouse.current.position.ReadValue().Deconstruct(out num, out num2);
			float num3 = num;
			float num4 = num2;
			int width = Screen.width;
			if (Math.Abs(width - 3840) > 1)
			{
				this._screenScale = 3840f / (float)width;
			}
			else
			{
				this._screenScale = 1f;
			}
			this._pointerX = num3 * this._screenScale;
			this._pointerY = num4 * this._screenScale;
			num2 = this._pointerX;
			bool flag;
			if (num2 > 0f && num2 < 3840f)
			{
				num2 = this._pointerY;
				flag = num2 > 0f && num2 < 600f;
			}
			else
			{
				flag = false;
			}
			this._pointerInHandZone = flag;
			num2 = this._pointerX;
			bool flag2;
			if (num2 > 0f && num2 < 3840f)
			{
				num2 = this._pointerY;
				flag2 = num2 > 600f && num2 < 1800f;
			}
			else
			{
				flag2 = false;
			}
			bool flag3 = flag2;
			switch (this._status)
			{
			case PlayBoard.InteractionStatus.Normal:
			{
				int? hoveringIndex = this.GetHoveringIndex();
				int? num5 = hoveringIndex;
				int? num6 = this._hoveringIndex;
				if (!((num5.GetValueOrDefault() == num6.GetValueOrDefault()) & (num5 != null == (num6 != null))))
				{
					List<HandCard> list = Enumerable.ToList<HandCard>(this.cardUi.GetUnpendingHands());
					num6 = this._hoveringIndex;
					if (num6 != null)
					{
						int valueOrDefault = num6.GetValueOrDefault();
						HandCard handCard = list.TryGetValue(valueOrDefault);
						if (handCard)
						{
							handCard.EndHover();
							UiManager.GetPanel<BattleManaPanel>().ClearHighlightMana();
						}
					}
					if (hoveringIndex != null)
					{
						int valueOrDefault2 = hoveringIndex.GetValueOrDefault();
						HandCard handCard2 = list.TryGetValue(valueOrDefault2);
						if (handCard2)
						{
							handCard2.StartHover();
							UiManager.GetPanel<BattleManaPanel>().SetCostHighlightForCard(handCard2.Card);
						}
					}
					this._hoveringIndex = hoveringIndex;
					return;
				}
				return;
			}
			case PlayBoard.InteractionStatus.CardSelected:
			case PlayBoard.InteractionStatus.CardTargetSelecting:
				if (this._pointerInUseZone == flag3)
				{
					return;
				}
				this._pointerInUseZone = flag3;
				if (this._pointerInUseZone)
				{
					this.EnterUseZone(this._activeHand);
					return;
				}
				this.LeaveUseZone(this._activeHand);
				return;
			case PlayBoard.InteractionStatus.UsTargetSelecting:
			case PlayBoard.InteractionStatus.DollTargetSelecting:
				return;
			}
			this._pointerInUseZone = false;
		}

		// Token: 0x060008F3 RID: 2291 RVA: 0x0002D9A8 File Offset: 0x0002BBA8
		public void ReverifyCard()
		{
			bool flag;
			if (this._status == PlayBoard.InteractionStatus.CardTargetSelecting && !this.UseCardVerify(this._activeHand.Card, true, out flag))
			{
				this.CancelTargetSelecting(true);
			}
		}

		// Token: 0x060008F4 RID: 2292 RVA: 0x0002D9DC File Offset: 0x0002BBDC
		private void EnterUseZone(HandCard handCard)
		{
			bool flag;
			if (this.UseCardVerify(handCard.Card, true, out flag))
			{
				BattleManaPanel panel = UiManager.GetPanel<BattleManaPanel>();
				handCard.CardWidget.SetCardEdge((flag && panel.CurrentKickerHighlighting) ? CardWidget.EdgeStatus.HighKicker : CardWidget.EdgeStatus.High);
				handCard.EndHover();
				handCard.Card.PendingManaUsage = new ManaGroup?(panel.HighlightingMana);
				this.targetSelector.EnterUseZone(handCard);
				this._status = PlayBoard.InteractionStatus.CardTargetSelecting;
				return;
			}
			handCard.CancelUse();
			this.CancelTargetSelecting(true);
		}

		// Token: 0x060008F5 RID: 2293 RVA: 0x0002DA58 File Offset: 0x0002BC58
		private void LeaveUseZone(HandCard handCard)
		{
			handCard.Card.PendingManaUsage = default(ManaGroup?);
			TargetType? targetType = handCard.Card.Config.TargetType;
			TargetType targetType2 = TargetType.SingleEnemy;
			if (!((targetType.GetValueOrDefault() == targetType2) & (targetType != null)))
			{
				bool flag2;
				bool flag = this.UseCardVerify(handCard.Card, false, out flag2);
				handCard.SetUsableStatus(flag, flag2);
				this.targetSelector.LeaveUseZone();
				this._status = PlayBoard.InteractionStatus.CardSelected;
			}
		}

		// Token: 0x060008F6 RID: 2294 RVA: 0x0002DACC File Offset: 0x0002BCCC
		private int? GetHoveringIndex()
		{
			if (!this.PlayerInTurn)
			{
				return default(int?);
			}
			if (this._pointerInHandZone)
			{
				Vector2 vector = new Vector2(this._pointerX, this._pointerY);
				for (int i = this.cardUi.UnpendingHandCount - 1; i >= 0; i--)
				{
					if (PlayBoard.<GetHoveringIndex>g__IsInCard|49_0(vector, this.cardUi.CardBaseV2List[i], this.cardUi.CardBaseRotateList[i]))
					{
						return new int?(i);
					}
				}
			}
			return default(int?);
		}

		// Token: 0x060008F7 RID: 2295 RVA: 0x0002DB58 File Offset: 0x0002BD58
		public void OnPointerEnter(PointerEventData eventData)
		{
			this._pointered = true;
			this.targetSelector.PlayBoardHasPointer = true;
		}

		// Token: 0x060008F8 RID: 2296 RVA: 0x0002DB70 File Offset: 0x0002BD70
		public void OnPointerExit(PointerEventData eventData)
		{
			this._pointered = false;
			this.targetSelector.PlayBoardHasPointer = false;
			PlayBoard.InteractionStatus status = this._status;
			if (status != PlayBoard.InteractionStatus.Normal)
			{
				if (status - PlayBoard.InteractionStatus.CardTargetSelecting > 3)
				{
					this.CancelTargetSelecting(true);
					this._pointerInHandZone = false;
					this._pointerInUseZone = false;
					this._status = PlayBoard.InteractionStatus.Normal;
				}
				return;
			}
			this.EndHoveringCard();
		}

		// Token: 0x060008F9 RID: 2297 RVA: 0x0002DBC8 File Offset: 0x0002BDC8
		private void EndHoveringCard()
		{
			List<HandCard> list = Enumerable.ToList<HandCard>(this.cardUi.GetUnpendingHands());
			int? hoveringIndex = this._hoveringIndex;
			if (hoveringIndex != null)
			{
				int valueOrDefault = hoveringIndex.GetValueOrDefault();
				HandCard handCard = list.TryGetValue(valueOrDefault);
				if (handCard)
				{
					handCard.EndHover();
					UiManager.GetPanel<BattleManaPanel>().ClearHighlightMana();
				}
			}
			this._hoveringIndex = default(int?);
		}

		// Token: 0x060008FA RID: 2298 RVA: 0x0002DC2C File Offset: 0x0002BE2C
		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				if (this.IsTempLockedFromMinimize)
				{
					return;
				}
				switch (this._status)
				{
				case PlayBoard.InteractionStatus.Normal:
				{
					int? num = this._hoveringIndex;
					if (num != null)
					{
						int valueOrDefault = num.GetValueOrDefault();
						HandCard handCard = Enumerable.ElementAt<HandCard>(this.cardUi.GetUnpendingHands(), valueOrDefault);
						this._activeHand = handCard;
						this._activeHand.IsActiveHand = true;
						this._status = PlayBoard.InteractionStatus.CardSelected;
						this._pointerUpUse = true;
						this.targetSelector.EnableSelector(this._activeHand);
						return;
					}
					return;
				}
				case PlayBoard.InteractionStatus.CardSelected:
					if (this._pointerInHandZone)
					{
						this.CancelTargetSelecting(true);
						return;
					}
					return;
				case PlayBoard.InteractionStatus.CardTargetSelecting:
					if (this._pointerInUseZone)
					{
						UnitSelector confirmUseSelector = this.targetSelector.GetConfirmUseSelector(eventData.position);
						if (confirmUseSelector != null)
						{
							this.ConfirmUseCardWithSelection(confirmUseSelector);
							return;
						}
						return;
					}
					else
					{
						TargetType? targetType = this._activeHand.Card.Config.TargetType;
						TargetType targetType2 = TargetType.SingleEnemy;
						if (!((targetType.GetValueOrDefault() == targetType2) & (targetType != null)))
						{
							this.CancelTargetSelecting(true);
							return;
						}
						return;
					}
					break;
				case PlayBoard.InteractionStatus.UsTargetSelecting:
				{
					UnitSelector confirmUseSelector2 = this.targetSelector.GetConfirmUseSelector(eventData.position);
					if (confirmUseSelector2 != null)
					{
						this.ConfirmUseUsWithSelection(confirmUseSelector2);
						this._status = PlayBoard.InteractionStatus.Normal;
						this.targetSelector.DisableSelector();
						return;
					}
					return;
				}
				case PlayBoard.InteractionStatus.DollTargetSelecting:
				{
					UnitSelector confirmUseSelector3 = this.targetSelector.GetConfirmUseSelector(eventData.position);
					if (confirmUseSelector3 != null)
					{
						this.ConfirmUseDollWithSelection(confirmUseSelector3);
						this._status = PlayBoard.InteractionStatus.Normal;
						this.targetSelector.DisableSelector();
						return;
					}
					return;
				}
				}
				throw new ArgumentOutOfRangeException();
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				PlayBoard.InteractionStatus status = this._status;
				if (status == PlayBoard.InteractionStatus.CardSelected || status == PlayBoard.InteractionStatus.CardTargetSelecting || status == PlayBoard.InteractionStatus.UsTargetSelecting || status == PlayBoard.InteractionStatus.DollTargetSelecting)
				{
					this.CancelTargetSelecting(true);
				}
				if (this._status == PlayBoard.InteractionStatus.Normal)
				{
					int? num = this._hoveringIndex;
					if (num != null)
					{
						int valueOrDefault2 = num.GetValueOrDefault();
						HandCard handCard2 = Enumerable.ElementAt<HandCard>(this.cardUi.GetUnpendingHands(), valueOrDefault2);
						AudioManager.Card(3);
						UiManager.GetPanel<CardDetailPanel>().Show(new CardDetailPayload(handCard2.GetComponent<RectTransform>(), handCard2.Card, true));
					}
				}
			}
		}

		// Token: 0x060008FB RID: 2299 RVA: 0x0002DE44 File Offset: 0x0002C044
		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				switch (this._status)
				{
				case PlayBoard.InteractionStatus.CardTargetSelecting:
					if (this._pointerUpUse && this._pointerInUseZone)
					{
						UnitSelector confirmUseSelector = this.targetSelector.GetConfirmUseSelector(eventData.position);
						if (confirmUseSelector != null)
						{
							this.ConfirmUseCardWithSelection(confirmUseSelector);
						}
					}
					break;
				case PlayBoard.InteractionStatus.UsTargetSelecting:
					if (this._pointerUpUse)
					{
						UnitSelector confirmUseSelector2 = this.targetSelector.GetConfirmUseSelector(eventData.position);
						if (confirmUseSelector2 != null)
						{
							this.ConfirmUseUsWithSelection(confirmUseSelector2);
						}
					}
					break;
				case PlayBoard.InteractionStatus.DollTargetSelecting:
					if (this._pointerUpUse)
					{
						UnitSelector confirmUseSelector3 = this.targetSelector.GetConfirmUseSelector(eventData.position);
						if (confirmUseSelector3 != null)
						{
							this.ConfirmUseDollWithSelection(confirmUseSelector3);
						}
					}
					break;
				}
				this._pointerUpUse = false;
			}
		}

		// Token: 0x060008FC RID: 2300 RVA: 0x0002DEFC File Offset: 0x0002C0FC
		private void InstantSelectCard(HandCard hand)
		{
			if (this._status != PlayBoard.InteractionStatus.Normal)
			{
				throw new InvalidOperationException(string.Format("Cannot instant select card while status is {0}", this._status));
			}
			this._activeHand = hand;
			this._activeHand.IsActiveHand = true;
			this._status = PlayBoard.InteractionStatus.CardSelected;
			UiManager.GetPanel<BattleManaPanel>().SetCostHighlightForCard(hand.Card);
			this.targetSelector.EnableSelector(this._activeHand);
			if (this._pointerInUseZone)
			{
				this.EnterUseZone(hand);
				this.targetSelector.ForceUpdate();
			}
		}

		// Token: 0x060008FD RID: 2301 RVA: 0x0002DF84 File Offset: 0x0002C184
		public bool HandleNavigateFromKey(NavigateDirection dir)
		{
			if (base.Battle == null)
			{
				return false;
			}
			List<HandCard> list = Enumerable.ToList<HandCard>(this.cardUi.GetUnpendingHands());
			if (list.Count == 0)
			{
				return false;
			}
			switch (this._status)
			{
			case PlayBoard.InteractionStatus.Inactive:
				return false;
			case PlayBoard.InteractionStatus.Normal:
				if (dir == NavigateDirection.Left)
				{
					List<HandCard> list2 = list;
					this.InstantSelectCard(list2[list2.Count - 1]);
				}
				else if (dir == NavigateDirection.Right)
				{
					this.InstantSelectCard(list[0]);
				}
				return true;
			case PlayBoard.InteractionStatus.CardSelected:
			case PlayBoard.InteractionStatus.CardTargetSelecting:
			{
				int num = list.IndexOf(this._activeHand);
				this.CancelTargetSelecting(true);
				if (num >= 0)
				{
					if (dir == NavigateDirection.Left)
					{
						HandCard handCard;
						if (num != 0)
						{
							handCard = list[num - 1];
						}
						else
						{
							List<HandCard> list3 = list;
							handCard = list3[list3.Count - 1];
						}
						this.InstantSelectCard(handCard);
					}
					else if (dir == NavigateDirection.Right)
					{
						this.InstantSelectCard((num + 1 >= list.Count) ? list[0] : list[num + 1]);
					}
				}
				else if (dir == NavigateDirection.Left)
				{
					List<HandCard> list4 = list;
					this.InstantSelectCard(list4[list4.Count - 1]);
				}
				else if (dir == NavigateDirection.Right)
				{
					this.InstantSelectCard(list[0]);
				}
				return true;
			}
			case PlayBoard.InteractionStatus.CardUsingConfirming:
				return false;
			case PlayBoard.InteractionStatus.UsTargetSelecting:
				return false;
			default:
				return false;
			}
		}

		// Token: 0x060008FE RID: 2302 RVA: 0x0002E0A7 File Offset: 0x0002C2A7
		public bool HandleConfirmAction()
		{
			BattleController battle = base.Battle;
			return false;
		}

		// Token: 0x060008FF RID: 2303 RVA: 0x0002E0B4 File Offset: 0x0002C2B4
		public bool HandleCancelAction()
		{
			if (this.IsTempLockedFromMinimize || base.Battle == null)
			{
				return false;
			}
			switch (this._status)
			{
			case PlayBoard.InteractionStatus.CardSelected:
				this.CancelTargetSelecting(true);
				return true;
			case PlayBoard.InteractionStatus.CardTargetSelecting:
				this.CancelTargetSelecting(true);
				return true;
			case PlayBoard.InteractionStatus.UsTargetSelecting:
				this.CancelTargetSelecting(true);
				return true;
			}
			return false;
		}

		// Token: 0x06000900 RID: 2304 RVA: 0x0002E110 File Offset: 0x0002C310
		public void HandleSelectionFromKey(int index)
		{
			if (this.IsTempLockedFromMinimize || base.Battle == null)
			{
				return;
			}
			List<HandCard> list = Enumerable.ToList<HandCard>(this.cardUi.GetUnpendingHands());
			switch (this._status)
			{
			case PlayBoard.InteractionStatus.Inactive:
			case PlayBoard.InteractionStatus.CardUsingConfirming:
				break;
			case PlayBoard.InteractionStatus.Normal:
				this.EndHoveringCard();
				if (index >= 0 && index < list.Count)
				{
					this.InstantSelectCard(list[index]);
					return;
				}
				break;
			case PlayBoard.InteractionStatus.CardSelected:
			case PlayBoard.InteractionStatus.CardTargetSelecting:
				if (index >= 0 && index < list.Count)
				{
					HandCard handCard = list[index];
					if (handCard != this._activeHand)
					{
						this.CancelTargetSelecting(true);
						this.InstantSelectCard(handCard);
						return;
					}
					this.CancelTargetSelecting(true);
					return;
				}
				break;
			case PlayBoard.InteractionStatus.UsTargetSelecting:
			case PlayBoard.InteractionStatus.DollTargetSelecting:
				if (index >= 0 && index < list.Count)
				{
					this.CancelTargetSelecting(true);
					this.InstantSelectCard(list[index]);
					return;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x0002E1ED File Offset: 0x0002C3ED
		public void HandlePoolMana(ManaColor color)
		{
			if (this.IsTempLockedFromMinimize || base.Battle == null)
			{
				return;
			}
			if (!UiManager.GetPanel<BattleManaPanel>().TryPoolSingle(color))
			{
				Debug.LogWarning(string.Format("Cannot pool mana {0}", color));
			}
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x0002E224 File Offset: 0x0002C424
		public void HandleUseUsFromKey()
		{
			if (this.IsTempLockedFromMinimize || base.Battle == null)
			{
				return;
			}
			switch (this._status)
			{
			case PlayBoard.InteractionStatus.Inactive:
			case PlayBoard.InteractionStatus.CardUsingConfirming:
			case PlayBoard.InteractionStatus.UsTargetSelecting:
			case PlayBoard.InteractionStatus.DollTargetSelecting:
				return;
			case PlayBoard.InteractionStatus.Normal:
				UiManager.GetPanel<UltimateSkillPanel>().UseUsFromKey();
				return;
			case PlayBoard.InteractionStatus.CardSelected:
			case PlayBoard.InteractionStatus.CardTargetSelecting:
				this.CancelTargetSelecting(true);
				UiManager.GetPanel<UltimateSkillPanel>().UseUsFromKey();
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x0002E290 File Offset: 0x0002C490
		public void HandleEndTurnFromKey()
		{
			if (this.IsTempLockedFromMinimize || base.Battle == null)
			{
				return;
			}
			if (this.endTurnButton.gameObject.activeSelf)
			{
				this.RequestEndTurn();
			}
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x0002E2BC File Offset: 0x0002C4BC
		public void EnableSelector(UltimateSkill us, Vector3 fromWorldPosition, bool fromClick)
		{
			if (this._status != PlayBoard.InteractionStatus.Normal)
			{
				Debug.LogError(string.Format("Cannot enable selector for US while status is {0}", this._status));
			}
			this._status = PlayBoard.InteractionStatus.UsTargetSelecting;
			if (fromClick)
			{
				this._pointerUpUse = true;
			}
			this.targetSelector.EnableSelector(us, fromWorldPosition);
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x0002E30C File Offset: 0x0002C50C
		public void EnableSelector(Doll doll, Vector3 fromWorldPosition, bool fromClick)
		{
			if (this._status != PlayBoard.InteractionStatus.Normal)
			{
				Debug.LogError(string.Format("Cannot enable selector for Doll while status is {0}", this._status));
			}
			this._activeDoll = GameDirector.Player.GetDoll(doll).InfoWidget;
			this._status = PlayBoard.InteractionStatus.DollTargetSelecting;
			if (fromClick)
			{
				this._pointerUpUse = true;
			}
			this.targetSelector.EnableSelector(doll, fromWorldPosition);
		}

		// Token: 0x06000906 RID: 2310 RVA: 0x0002E370 File Offset: 0x0002C570
		public void CancelTargetSelecting(bool refreshCards = true)
		{
			PlayBoard.InteractionStatus status = this._status;
			if (status != PlayBoard.InteractionStatus.CardSelected && status != PlayBoard.InteractionStatus.CardTargetSelecting && status != PlayBoard.InteractionStatus.CardUsingConfirming && status != PlayBoard.InteractionStatus.UsTargetSelecting && status != PlayBoard.InteractionStatus.DollTargetSelecting)
			{
				return;
			}
			switch (this._status)
			{
			case PlayBoard.InteractionStatus.CardSelected:
			case PlayBoard.InteractionStatus.CardTargetSelecting:
			case PlayBoard.InteractionStatus.CardUsingConfirming:
				if (refreshCards)
				{
					this.cardUi.RefreshAllCardsEdge();
				}
				if (this._activeHand != null)
				{
					this._activeHand.Card.PendingManaUsage = default(ManaGroup?);
					this._activeHand.CancelUse();
					this._activeHand = null;
				}
				UiManager.GetPanel<BattleManaPanel>().ClearHighlightMana();
				break;
			case PlayBoard.InteractionStatus.UsTargetSelecting:
				UiManager.GetPanel<UltimateSkillPanel>().CancelUse();
				break;
			case PlayBoard.InteractionStatus.DollTargetSelecting:
				if (this._activeDoll != null)
				{
					this._activeDoll.CancelUse();
					this._activeDoll = null;
				}
				break;
			}
			this.targetSelector.DisableSelector();
			this._status = PlayBoard.InteractionStatus.Normal;
			this._pointerUpUse = false;
			this.EndHoveringCard();
		}

		// Token: 0x06000907 RID: 2311 RVA: 0x0002E45C File Offset: 0x0002C65C
		public void CancelTargetSelectingIfCardIs(HandCard hand)
		{
			if (hand == this._activeHand)
			{
				Debug.Log("[PlayBoard] Cancel target selecting because hand is " + hand.Card.DebugName + ", maybe moving/discarding/exiling");
				this.CancelTargetSelecting(true);
			}
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x0002E494 File Offset: 0x0002C694
		protected override void OnEnterBattle()
		{
			base.Battle.WaitingPlayerInput += new Action(this.OnWaitingPlayerInput);
			base.Battle.ActionViewer.Register<StartBattleAction>(new BattleActionViewer<StartBattleAction>(this.ViewStartBattle), null);
			base.Battle.ActionViewer.Register<StartPlayerTurnAction>(new BattleActionViewer<StartPlayerTurnAction>(this.ViewStartPlayerTurn), null);
			base.Battle.ActionViewer.Register<StartAllEnemyTurnAction>(new BattleActionViewer<StartAllEnemyTurnAction>(this.ViewStartAllEnemyTurn), null);
			base.Battle.ActionViewer.Register<EndBattleAction>(new BattleActionViewer<EndBattleAction>(this.ViewEndBattle), null);
			base.Battle.ActionViewer.Register<EndPlayerTurnAction>(new BattleActionViewer<EndPlayerTurnAction>(this.ViewEndPlayerTurn), null);
			this.cardUi.EnterBattle(base.Battle);
			base.Battle.Notification += new Action<BattleMessage>(this.OnNotification);
			this._status = PlayBoard.InteractionStatus.Normal;
			this._hoveringIndex = default(int?);
			this.IsTempLockedFromMinimize = false;
			base.Battle.GlobalStatusChanged += new Action(this.OnGlobalStatusChanged);
			base.Show();
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x0002E5A8 File Offset: 0x0002C7A8
		protected override void OnLeaveBattle()
		{
			if (this._status != PlayBoard.InteractionStatus.Normal)
			{
				this.CancelTargetSelecting(false);
			}
			base.Battle.WaitingPlayerInput -= new Action(this.OnWaitingPlayerInput);
			base.Battle.ActionViewer.Unregister<StartBattleAction>(new BattleActionViewer<StartBattleAction>(this.ViewStartBattle));
			base.Battle.ActionViewer.Unregister<StartPlayerTurnAction>(new BattleActionViewer<StartPlayerTurnAction>(this.ViewStartPlayerTurn));
			base.Battle.ActionViewer.Unregister<StartAllEnemyTurnAction>(new BattleActionViewer<StartAllEnemyTurnAction>(this.ViewStartAllEnemyTurn));
			base.Battle.ActionViewer.Unregister<EndBattleAction>(new BattleActionViewer<EndBattleAction>(this.ViewEndBattle));
			base.Battle.ActionViewer.Unregister<EndPlayerTurnAction>(new BattleActionViewer<EndPlayerTurnAction>(this.ViewEndPlayerTurn));
			this.cardUi.LeaveBattle(base.Battle);
			base.Battle.Notification -= new Action<BattleMessage>(this.OnNotification);
			this._status = PlayBoard.InteractionStatus.Inactive;
			base.Battle.GlobalStatusChanged -= new Action(this.OnGlobalStatusChanged);
			base.Hide();
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x0002E6B4 File Offset: 0x0002C8B4
		private void OnGlobalStatusChanged()
		{
			this.cardUi.RefreshAll();
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x0600090B RID: 2315 RVA: 0x0002E6C1 File Offset: 0x0002C8C1
		// (set) Token: 0x0600090C RID: 2316 RVA: 0x0002E6C9 File Offset: 0x0002C8C9
		private bool PlayerInTurn { get; set; }

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x0600090D RID: 2317 RVA: 0x0002E6D2 File Offset: 0x0002C8D2
		// (set) Token: 0x0600090E RID: 2318 RVA: 0x0002E6DA File Offset: 0x0002C8DA
		private Coroutine PlayerInTurnSetter { get; set; }

		// Token: 0x0600090F RID: 2319 RVA: 0x0002E6E3 File Offset: 0x0002C8E3
		private IEnumerator ViewStartBattle(StartBattleAction action)
		{
			UiManager.GetPanel<SystemBoard>().SetBattleStatus(BattleStatus.BattleStart);
			yield return UiManager.GetPanel<BattleNotifier>().ShowBattleStart();
			GameDirector.FadeInEnemyStatus();
			this._playerContinousTurnCounter = 0;
			yield break;
		}

		// Token: 0x06000910 RID: 2320 RVA: 0x0002E6F2 File Offset: 0x0002C8F2
		private IEnumerator ViewStartPlayerTurn(StartPlayerTurnAction action)
		{
			UiManager.GetPanel<SystemBoard>().SetBattleStatus(BattleStatus.PlayerTurn);
			yield return UiManager.GetPanel<BattleNotifier>().ShowPlayerTurn(base.Battle.RoundCounter, action.IsExtra);
			this.endTurnButton.gameObject.SetActive(true);
			this.endTurnButton.GetComponent<Image>().DOFade(1f, 0.3f).From(0f, true, false)
				.SetLink(base.gameObject);
			this.PlayerInTurnSetter = base.StartCoroutine("PlayerInTurnRunner");
			if (!this._requests.Empty<PlayBoard.RequestEntry>())
			{
				Debug.Log("Start player turn with requests not cleared: [" + string.Join(", ", Enumerable.Select<PlayBoard.RequestEntry, string>(this._requests, (PlayBoard.RequestEntry r) => r.GetType().Name)) + "]");
				this._requests.Clear();
			}
			if (action.IsExtra)
			{
				this._playerContinousTurnCounter++;
				if (base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>() && this._playerContinousTurnCounter >= 5)
				{
					GameMaster.UnlockAchievement(AchievementKey.MyTurn);
				}
			}
			else
			{
				this._playerContinousTurnCounter = 1;
			}
			yield break;
		}

		// Token: 0x06000911 RID: 2321 RVA: 0x0002E708 File Offset: 0x0002C908
		private IEnumerator ViewEndPlayerTurn(EndPlayerTurnAction action)
		{
			this.CancelTargetSelecting(true);
			this.RewindRequests();
			this.endTurnButton.gameObject.SetActive(false);
			if (this.PlayerInTurnSetter != null)
			{
				base.StopCoroutine(this.PlayerInTurnSetter);
				this.PlayerInTurnSetter = null;
			}
			this.PlayerInTurn = false;
			yield break;
		}

		// Token: 0x06000912 RID: 2322 RVA: 0x0002E717 File Offset: 0x0002C917
		private IEnumerator PlayerInTurnRunner()
		{
			yield return new WaitForSecondsRealtime(0.3f);
			this.PlayerInTurn = true;
			yield break;
		}

		// Token: 0x06000913 RID: 2323 RVA: 0x0002E726 File Offset: 0x0002C926
		private IEnumerator ViewStartAllEnemyTurn(StartAllEnemyTurnAction action)
		{
			UiManager.GetPanel<SystemBoard>().SetBattleStatus(BattleStatus.EnemyTurn);
			yield return UiManager.GetPanel<BattleNotifier>().ShowEnemyTurn(base.Battle.RoundCounter);
			yield break;
		}

		// Token: 0x06000914 RID: 2324 RVA: 0x0002E735 File Offset: 0x0002C935
		private IEnumerator ViewEndBattle(EndBattleAction action)
		{
			UiManager.GetPanel<SystemBoard>().SetBattleStatus(BattleStatus.OutOfBattle);
			yield break;
		}

		// Token: 0x06000915 RID: 2325 RVA: 0x0002E73D File Offset: 0x0002C93D
		private IEnumerator ViewStartEnemyTurn(StartEnemyTurnAction action)
		{
			if (action.Unit.IsExtraTurn)
			{
				Debug.Log(string.Format("View {0} extra-turn (turn {1})", action.Unit.DebugName, action.Unit.TurnCounter));
			}
			yield break;
		}

		// Token: 0x06000916 RID: 2326 RVA: 0x0002E74C File Offset: 0x0002C94C
		private void OnNotification(BattleMessage message)
		{
			switch (message)
			{
			case BattleMessage.HandFull:
				this.ShowErrorChat(this._handFull);
				return;
			case BattleMessage.ZoneFull:
			case BattleMessage.DollSlotFull:
			case BattleMessage.DollSlotEmpty:
			case BattleMessage.DollSlotNotEnough:
				Debug.Log(string.Format("[PlayBoard] Show battle message for {0}", message));
				return;
			case BattleMessage.EmptyDraw:
				this.ShowErrorChat(this._emptyDraw);
				return;
			default:
				throw new ArgumentOutOfRangeException("message", message, null);
			}
		}

		// Token: 0x06000917 RID: 2327 RVA: 0x0002E7BC File Offset: 0x0002C9BC
		private void ConfirmUseCardWithSelection(UnitSelector selector)
		{
			PlayBoard.<>c__DisplayClass86_0 CS$<>8__locals1 = new PlayBoard.<>c__DisplayClass86_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.selector = selector;
			CS$<>8__locals1.card = this._activeHand.Card;
			BattleManaPanel panel = UiManager.GetPanel<BattleManaPanel>();
			if (!panel.TryGetConfirmUseMana(CS$<>8__locals1.card, out CS$<>8__locals1.kicker, out CS$<>8__locals1.consuming))
			{
				this.ShowErrorChat(this._cardCostChanged);
				panel.ClearHighlightMana();
				panel.SetCostHighlightForCard(CS$<>8__locals1.card);
				bool flag;
				if (this.UseCardVerify(CS$<>8__locals1.card, false, out flag))
				{
					CS$<>8__locals1.card.PendingManaUsage = new ManaGroup?(panel.HighlightingMana);
					return;
				}
				this.CancelTargetSelecting(true);
				return;
			}
			else
			{
				if (CS$<>8__locals1.card.IsXCost && CS$<>8__locals1.consuming.Pooled.IsEmpty && GameMaster.ShowXCostEmptyUseWarning)
				{
					this._status = PlayBoard.InteractionStatus.CardUsingConfirming;
					UiManager.GetDialog<MessageDialog>().Show(new MessageContent
					{
						TextKey = "XCostEmptyUsageWarning",
						TextArguments = new object[] { CS$<>8__locals1.card.Name },
						SubTextKey = "XCostEmptyUsageTips",
						Icon = MessageIcon.Warning,
						Buttons = DialogButtons.ConfirmCancel,
						OnCancel = delegate
						{
							CS$<>8__locals1.<>4__this.CancelTargetSelecting(true);
						},
						OnConfirm = new Action(CS$<>8__locals1.<ConfirmUseCardWithSelection>g__InternalConfirmUseCard|0)
					});
					Cursor.visible = true;
					return;
				}
				CS$<>8__locals1.<ConfirmUseCardWithSelection>g__InternalConfirmUseCard|0();
				return;
			}
		}

		// Token: 0x06000918 RID: 2328 RVA: 0x0002E920 File Offset: 0x0002CB20
		private void ConfirmUseUsWithSelection(UnitSelector selector)
		{
			UltimateSkill us = base.GameRun.Player.Us;
			if (us.TargetType != TargetType.SingleEnemy)
			{
				Debug.LogError(string.Format("US {0} should not invoke with selector: {1}", us.DebugName, selector));
				return;
			}
			this.RequestUseUs(selector);
			this._status = PlayBoard.InteractionStatus.Normal;
			this.targetSelector.DisableSelector();
		}

		// Token: 0x06000919 RID: 2329 RVA: 0x0002E978 File Offset: 0x0002CB78
		private void ConfirmUseDollWithSelection(UnitSelector selector)
		{
			Doll doll = this._activeDoll.Doll;
			if (doll.TargetType != TargetType.SingleEnemy)
			{
				Debug.LogError(string.Format("Doll {0} should not invoke with selector: {1}", doll.DebugName, selector));
				return;
			}
			this.RequestUseDoll(doll, selector);
			this._status = PlayBoard.InteractionStatus.Normal;
			this.targetSelector.DisableSelector();
		}

		// Token: 0x0600091A RID: 2330 RVA: 0x0002E9CB File Offset: 0x0002CBCB
		private void UI_EndTurn()
		{
			if (!this.IsTempLockedFromMinimize)
			{
				this.RequestEndTurn();
			}
		}

		// Token: 0x0600091B RID: 2331 RVA: 0x0002E9DC File Offset: 0x0002CBDC
		public bool UseCardVerify(Card card, bool showErrorChat, out bool kickerUsable)
		{
			kickerUsable = false;
			if (!base.Battle.Player.IsInTurn)
			{
				return false;
			}
			if (card.Zone != CardZone.Hand)
			{
				if (showErrorChat)
				{
					this.ShowErrorChat(this._cardNotInHand);
				}
				return false;
			}
			Card card2;
			if (base.Battle.DoesHandCardPreventUse(card, out card2))
			{
				if (showErrorChat)
				{
					card2.NotifyActivating();
					this.ShowErrorChat(card2.PreventCardUsageMessage);
				}
				return false;
			}
			StatusEffect statusEffect;
			if (base.Battle.DoesPlayerStatusEffectForbidUse(card, out statusEffect))
			{
				if (showErrorChat)
				{
					statusEffect.NotifyActivating();
					this.ShowErrorChat(statusEffect.PreventCardUsageMessage);
				}
				return false;
			}
			if (card.IsForbidden || !card.CanUse)
			{
				if (showErrorChat)
				{
					this.ShowErrorChat(card.CantUseMessage);
				}
				return false;
			}
			int? moneyCost = card.Config.MoneyCost;
			if (moneyCost != null)
			{
				int valueOrDefault = moneyCost.GetValueOrDefault();
				if (base.GameRun.Money < valueOrDefault)
				{
					if (showErrorChat)
					{
						this.ShowMoneyNotEnough();
					}
					return false;
				}
			}
			if (card.Summoned && card.Loyalty < card.MinLoyaltyToUseSkill)
			{
				if (showErrorChat)
				{
					this.ShowLowLoyalty();
				}
				return false;
			}
			ManaGroup availableMana = UiManager.GetPanel<BattleManaPanel>().AvailableMana;
			if (!availableMana.CanAfford(card.IsXCost ? card.XCostRequiredMana : card.Cost))
			{
				if (showErrorChat)
				{
					this.ShowLowMana();
				}
				return false;
			}
			if (card.HasKicker && availableMana.CanAfford(card.KickerTotalCost))
			{
				kickerUsable = true;
			}
			return true;
		}

		// Token: 0x0600091C RID: 2332 RVA: 0x0002EB34 File Offset: 0x0002CD34
		private bool UseCardVerify(Card card, UnitSelector selector, ConsumingMana consumingMana, bool isCommitting)
		{
			if (!base.Battle.Player.IsInTurn)
			{
				return false;
			}
			if (card.Zone != CardZone.Hand)
			{
				this.ShowErrorChat(this._cardNotInHand);
				return false;
			}
			if (selector.Type == TargetType.SingleEnemy)
			{
				EnemyUnit selectedEnemy = selector.SelectedEnemy;
				if (selectedEnemy != null && selectedEnemy.IsDead)
				{
					this.ShowErrorChat(this._targetAlreadyDead);
					return false;
				}
			}
			Card card2;
			if (base.Battle.DoesHandCardPreventUse(card, out card2))
			{
				card2.NotifyActivating();
				this.ShowErrorChat(card2.PreventCardUsageMessage);
				return false;
			}
			StatusEffect statusEffect;
			if (base.Battle.DoesPlayerStatusEffectForbidUse(card, out statusEffect))
			{
				statusEffect.NotifyActivating();
				this.ShowErrorChat(statusEffect.PreventCardUsageMessage);
				return false;
			}
			if (card.IsForbidden || !card.CanUse)
			{
				this.ShowErrorChat(card.CantUseMessage);
				return false;
			}
			int? moneyCost = card.Config.MoneyCost;
			if (moneyCost != null)
			{
				int valueOrDefault = moneyCost.GetValueOrDefault();
				if (base.GameRun.Money < valueOrDefault)
				{
					this.ShowMoneyNotEnough();
					return false;
				}
			}
			if (card.Summoned && card.Loyalty < card.MinLoyaltyToUseSkill)
			{
				this.ShowLowLoyalty();
				return false;
			}
			if (!(isCommitting ? base.Battle.BattleMana : UiManager.GetPanel<BattleManaPanel>().AvailableMana).CanAfford(consumingMana.TotalMana))
			{
				this.ShowLowMana();
				return false;
			}
			return true;
		}

		// Token: 0x0600091D RID: 2333 RVA: 0x0002EC84 File Offset: 0x0002CE84
		private bool UseUsVerify(UnitSelector selector)
		{
			if (!base.Battle.Player.HasUs)
			{
				this.ShowErrorChat("Player doesn't has US");
				return false;
			}
			UltimateSkill us = base.Battle.Player.Us;
			if (base.Battle.Player.Power < us.PowerCost)
			{
				this.ShowLowPower();
				return false;
			}
			if (!us.Available)
			{
				if (us.BattleAvailable)
				{
					this.ShowUsUsedThisTurn();
				}
				else
				{
					this.ShowUsUsedThisBattle();
				}
				return false;
			}
			if (selector.Type == TargetType.SingleEnemy)
			{
				EnemyUnit selectedEnemy = selector.SelectedEnemy;
				if (selectedEnemy != null && selectedEnemy.IsDead)
				{
					this.ShowErrorChat(this._targetAlreadyDead);
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600091E RID: 2334 RVA: 0x0002ED2C File Offset: 0x0002CF2C
		private bool UseDollVerify(Doll doll, UnitSelector selector)
		{
			if (!base.Battle.Player.IsInTurn || !doll.Usable)
			{
				return false;
			}
			if (!Enumerable.Contains<Doll>(base.Battle.Player.Dolls, doll))
			{
				this.ShowErrorChat("Player doesn't has Doll:" + doll.DebugName);
				return false;
			}
			if (doll.HasMagic && doll.Magic < doll.MagicCost)
			{
				this.ShowLowMagic();
				return false;
			}
			if (selector.Type == TargetType.SingleEnemy)
			{
				EnemyUnit selectedEnemy = selector.SelectedEnemy;
				if (selectedEnemy != null && selectedEnemy.IsDead)
				{
					this.ShowErrorChat(this._targetAlreadyDead);
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600091F RID: 2335 RVA: 0x0002EDCD File Offset: 0x0002CFCD
		private void RewindCard(Card card, ConsumingMana consumingMana)
		{
			this.cardUi.CancelUse(card);
			UiManager.GetPanel<BattleManaPanel>().RefundBack(consumingMana);
		}

		// Token: 0x06000920 RID: 2336 RVA: 0x0002EDE6 File Offset: 0x0002CFE6
		private void RewindUs()
		{
			UiManager.GetPanel<UltimateSkillPanel>().CancelUse();
		}

		// Token: 0x06000921 RID: 2337 RVA: 0x0002EDF2 File Offset: 0x0002CFF2
		private void RewindDoll(Doll doll)
		{
			DollView doll2 = GameDirector.Player.GetDoll(doll);
			if (doll2 == null)
			{
				return;
			}
			DollInfoWidget infoWidget = doll2.InfoWidget;
			if (infoWidget == null)
			{
				return;
			}
			infoWidget.CancelUse();
		}

		// Token: 0x06000922 RID: 2338 RVA: 0x0002EE13 File Offset: 0x0002D013
		public void SetEndTurnParticle(bool active)
		{
			if (!base.GameRun.Battle.IsWaitingPlayerInput)
			{
				return;
			}
			this.endTurnEdgeCg.DOFade((float)(active ? 1 : 0), 0.33f).SetLink(this.endTurnEdgeCg.gameObject);
		}

		// Token: 0x06000923 RID: 2339 RVA: 0x0002EE51 File Offset: 0x0002D051
		private void ShowErrorChat(string info)
		{
			GameDirector.Player.Chat(info, 3f, ChatWidget.CloudType.LeftThink, 0f);
		}

		// Token: 0x06000924 RID: 2340 RVA: 0x0002EE69 File Offset: 0x0002D069
		public void ShowMoneyNotEnough()
		{
			this.ShowErrorChat(this._moneyNotEnough);
		}

		// Token: 0x06000925 RID: 2341 RVA: 0x0002EE77 File Offset: 0x0002D077
		public void ShowLowMana()
		{
			this.ShowErrorChat(this._lowMana);
		}

		// Token: 0x06000926 RID: 2342 RVA: 0x0002EE85 File Offset: 0x0002D085
		public void ShowLowPower()
		{
			this.ShowErrorChat(this._lowPower);
		}

		// Token: 0x06000927 RID: 2343 RVA: 0x0002EE93 File Offset: 0x0002D093
		public void ShowLowMagic()
		{
			this.ShowErrorChat(this._lowMagic);
		}

		// Token: 0x06000928 RID: 2344 RVA: 0x0002EEA1 File Offset: 0x0002D0A1
		public void ShowLowLoyalty()
		{
			this.ShowErrorChat(this._lowLoyalty);
		}

		// Token: 0x06000929 RID: 2345 RVA: 0x0002EEAF File Offset: 0x0002D0AF
		public void ShowUsUsedThisTurn()
		{
			this.ShowErrorChat(this._usUsedThisTurn);
		}

		// Token: 0x0600092A RID: 2346 RVA: 0x0002EEBD File Offset: 0x0002D0BD
		public void ShowUsUsedThisBattle()
		{
			this.ShowErrorChat(this._usUsedThisBattle);
		}

		// Token: 0x0600092B RID: 2347 RVA: 0x0002EECC File Offset: 0x0002D0CC
		public void ShowDrawZone()
		{
			if (base.Battle == null)
			{
				return;
			}
			ShowCardsPayload showCardsPayload = new ShowCardsPayload
			{
				Name = "Game.DrawZoneOutOfOrder".Localize(true),
				Description = "Cards.Show".Localize(true),
				Cards = base.Battle.DrawZone,
				InteractionType = InteractionType.None,
				CardZone = ShowCardZone.Draw,
				HideActualOrder = (base.GameRun.CanViewDrawZoneActualOrder <= 0)
			};
			UiManager.GetPanel<ShowCardsPanel>().Show(showCardsPayload);
		}

		// Token: 0x0600092C RID: 2348 RVA: 0x0002EF4C File Offset: 0x0002D14C
		public void ShowDiscardZone()
		{
			if (base.Battle == null)
			{
				return;
			}
			ShowCardsPayload showCardsPayload = new ShowCardsPayload
			{
				Name = "Game.DiscardZone".Localize(true),
				Description = "Cards.Show".Localize(true),
				Cards = base.Battle.DiscardZone,
				InteractionType = InteractionType.None,
				CardZone = ShowCardZone.Discard
			};
			UiManager.GetPanel<ShowCardsPanel>().Show(showCardsPayload);
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x0002EFB4 File Offset: 0x0002D1B4
		public void ShowExileZone()
		{
			if (base.Battle == null)
			{
				return;
			}
			ShowCardsPayload showCardsPayload = new ShowCardsPayload
			{
				Name = "Game.ExileZone".Localize(true),
				Description = "Cards.Show".Localize(true),
				Cards = base.Battle.ExileZone,
				InteractionType = InteractionType.None,
				CardZone = ShowCardZone.Exile
			};
			UiManager.GetPanel<ShowCardsPanel>().Show(showCardsPayload);
		}

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x0600092E RID: 2350 RVA: 0x0002F01C File Offset: 0x0002D21C
		// (set) Token: 0x0600092F RID: 2351 RVA: 0x0002F024 File Offset: 0x0002D224
		public bool IsTempLockedFromMinimize
		{
			get
			{
				return this._isTempLockedFromMinimize;
			}
			set
			{
				this._isTempLockedFromMinimize = value;
				this.cardUi.SetPendingCardsAlpha(value ? 0.2f : 1f);
			}
		}

		// Token: 0x06000930 RID: 2352 RVA: 0x0002F048 File Offset: 0x0002D248
		public ManaGroup GetHandRemainCostExcept(Card exceptCard)
		{
			ManaGroup manaGroup = ManaGroup.Empty;
			foreach (HandCard handCard in this.cardUi.GetUnpendingHands())
			{
				Card card = handCard.Card;
				if (card != exceptCard)
				{
					if (card.IsXCost)
					{
						manaGroup += card.XCostRequiredMana;
					}
					else
					{
						manaGroup += card.Cost;
					}
				}
			}
			return manaGroup;
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x0002F0C8 File Offset: 0x0002D2C8
		public Vector3? FindActionSourceWorldPosition(GameEntity source)
		{
			Card card = source as Card;
			if (card != null)
			{
				HandCard handCard = this.cardUi.FindHandWidget(card);
				if (handCard)
				{
					return new Vector3?(handCard.TargetWorldPosition);
				}
				CardWidget cardWidget = this.cardUi.FindFollowPlayWidget(card);
				if (cardWidget)
				{
					return new Vector3?(cardWidget.transform.position);
				}
				cardWidget = this.cardUi.FindViewWidget(card);
				if (cardWidget)
				{
					return new Vector3?(cardWidget.transform.position);
				}
			}
			else
			{
				StatusEffect statusEffect = source as StatusEffect;
				if (statusEffect != null)
				{
					Unit owner = statusEffect.Owner;
					if (owner == null)
					{
						return default(Vector3?);
					}
					UnitView unit = GameDirector.GetUnit(owner);
					if (unit)
					{
						return unit.FindStatusEffectWorldPosition(statusEffect);
					}
					Debug.LogError("[PlayBoard] Cannot get SE action source owner " + statusEffect.Owner.DebugName + " from UnitView");
				}
				else
				{
					Exhibit exhibit = source as Exhibit;
					if (exhibit != null)
					{
						ExhibitWidget exhibitWidget = UiManager.GetPanel<SystemBoard>().FindExhibit(exhibit);
						if (exhibitWidget)
						{
							return new Vector3?(exhibitWidget.CenterWorldPosition);
						}
					}
					else if (source is PlayerUnit)
					{
						UnitView player = GameDirector.Player;
						if (player)
						{
							return new Vector3?(CameraController.ScenePositionToWorldPositionInUI(player.transform.position));
						}
					}
					else
					{
						EnemyUnit enemyUnit = source as EnemyUnit;
						if (enemyUnit != null)
						{
							UnitView enemy = GameDirector.GetEnemy(enemyUnit);
							if (enemy)
							{
								return new Vector3?(CameraController.ScenePositionToWorldPositionInUI(enemy.transform.position));
							}
						}
						else
						{
							Doll doll = source as Doll;
							if (doll != null)
							{
								UnitView player2 = GameDirector.Player;
								if (player2)
								{
									DollView doll2 = player2.GetDoll(doll);
									if (doll2)
									{
										return new Vector3?(CameraController.ScenePositionToWorldPositionInUI(doll2.transform.position));
									}
								}
							}
						}
					}
				}
			}
			return default(Vector3?);
		}

		// Token: 0x06000932 RID: 2354 RVA: 0x0002F29A File Offset: 0x0002D49A
		public void SetCursorVisible()
		{
			this.targetSelector.SetCursorVisible();
		}

		// Token: 0x06000933 RID: 2355 RVA: 0x0002F2A8 File Offset: 0x0002D4A8
		private void InitEffects()
		{
			this._manaConsumeEffectPool = this.CreateEffectPool<Transform>(this.manaConsumeEffect, 3f, delegate(Transform g)
			{
				this._manaConsumeEffectPool.Release(g);
			});
			this._manaLoseEffectPool = this.CreateEffectPool<Transform>(this.manaLoseEffect, 1f, delegate(Transform g)
			{
				this._manaLoseEffectPool.Release(g);
			});
			this._manaFlyEffectPool = this.CreateEffectPool<ManaFlyEffect>(this.manaFlyEffect, 1f, delegate(ManaFlyEffect g)
			{
				this._manaFlyEffectPool.Release(g);
			});
			this.RecycleEffectsAsync();
		}

		// Token: 0x06000934 RID: 2356 RVA: 0x0002F328 File Offset: 0x0002D528
		private ObjectPool<T> CreateEffectPool<T>(T template, float duration, Action<T> releaseAction) where T : Component
		{
			return new ObjectPool<T>(() => Object.Instantiate<T>(template, this.effectLayer, false), delegate(T g)
			{
				g.gameObject.SetActive(true);
				this._effectTimeoutActionQueue.Enqueue(delegate
				{
					releaseAction.Invoke(g);
				}, Time.time + duration);
			}, delegate(T g)
			{
				if (g)
				{
					g.gameObject.SetActive(false);
				}
			}, new Action<T>(Object.Destroy), true, 10, 10000);
		}

		// Token: 0x06000935 RID: 2357 RVA: 0x0002F3A8 File Offset: 0x0002D5A8
		private async UniTaskVoid RecycleEffectsAsync()
		{
			for (;;)
			{
				Action action;
				float num;
				if (this._effectTimeoutActionQueue.TryPeek(out action, out num))
				{
					if (num < Time.time)
					{
						this._effectTimeoutActionQueue.Dequeue();
						action.Invoke();
					}
					else
					{
						await UniTask.Yield();
					}
				}
				else
				{
					await UniTask.Yield();
				}
			}
		}

		// Token: 0x06000936 RID: 2358 RVA: 0x0002F3EC File Offset: 0x0002D5EC
		public float PlayManaGainEffect(ManaColor color, Vector3? fromPosition, Vector3 toPosition)
		{
			this._manaConsumeEffectPool.Get().position = toPosition;
			AudioManager.PlayUi("ManaGain", false);
			if (fromPosition != null)
			{
				Vector3 valueOrDefault = fromPosition.GetValueOrDefault();
				ManaFlyEffect manaFlyEffect = this._manaFlyEffectPool.Get();
				manaFlyEffect.SetColor(color);
				Vector3 vector = this.effectLayer.InverseTransformPoint(valueOrDefault);
				Vector3 vector2 = this.effectLayer.InverseTransformPoint(toPosition);
				manaFlyEffect.transform.localPosition = vector;
				Vector3 vector3 = Vector3.Lerp(vector, vector2, 0.3f) - new Vector3(100f, 0f);
				Vector3[] array = new Vector3[] { vector3, vector2 };
				manaFlyEffect.transform.DOLocalPath(array, 0.3f, PathType.CatmullRom, PathMode.Full3D, 10, default(Color?)).SetEase(Ease.OutSine).SetUpdate(true);
				AudioManager.PlayUi("ManaFly", false);
			}
			return 0.2f;
		}

		// Token: 0x06000937 RID: 2359 RVA: 0x0002F4DC File Offset: 0x0002D6DC
		public float PlayManaConsumeEffect(ManaColor color, Vector3 fromPosition, Vector3? toPosition)
		{
			this._manaConsumeEffectPool.Get().position = fromPosition;
			AudioManager.PlayUi("ManaConsume", false);
			if (toPosition != null)
			{
				Vector3 valueOrDefault = toPosition.GetValueOrDefault();
				ManaFlyEffect manaFlyEffect = this._manaFlyEffectPool.Get();
				manaFlyEffect.SetColor(color);
				Vector3 vector = this.effectLayer.InverseTransformPoint(fromPosition);
				Vector3 vector2 = this.effectLayer.InverseTransformPoint(valueOrDefault);
				vector2 += new Vector3(220f, 330f);
				manaFlyEffect.transform.localPosition = vector;
				Vector3 vector3 = Vector3.Lerp(vector, vector2, 0.5f) - new Vector3(0f, 100f);
				Vector3[] array = new Vector3[] { vector3, vector2 };
				manaFlyEffect.transform.DOLocalPath(array, 0.3f, PathType.CatmullRom, PathMode.Full3D, 10, default(Color?)).SetEase(Ease.OutQuad).SetUpdate(true);
				AudioManager.PlayUi("ManaFly", false);
			}
			return 0.2f;
		}

		// Token: 0x06000938 RID: 2360 RVA: 0x0002F5E0 File Offset: 0x0002D7E0
		public float PlayManaLoseEffect(ManaColor color, Vector3 fromPosition, Vector3? toPosition)
		{
			this._manaLoseEffectPool.Get().position = fromPosition;
			AudioManager.PlayUi("ManaLose", false);
			if (toPosition != null)
			{
				Vector3 valueOrDefault = toPosition.GetValueOrDefault();
				ManaFlyEffect manaFlyEffect = this._manaFlyEffectPool.Get();
				manaFlyEffect.SetColor(color);
				Vector3 vector = this.effectLayer.InverseTransformPoint(fromPosition);
				Vector3 vector2 = this.effectLayer.InverseTransformPoint(valueOrDefault);
				manaFlyEffect.transform.localPosition = vector;
				Vector3 vector3 = Vector3.Lerp(vector, vector2, 0.5f) - new Vector3(0f, 100f);
				Vector3[] array = new Vector3[] { vector3, vector2 };
				manaFlyEffect.transform.DOLocalPath(array, 0.3f, PathType.CatmullRom, PathMode.Full3D, 10, default(Color?)).SetUpdate(true);
				AudioManager.PlayUi("ManaFly", false);
			}
			return 0.2f;
		}

		// Token: 0x06000939 RID: 2361 RVA: 0x0002F6C8 File Offset: 0x0002D8C8
		private bool RequestUseCard(Card card, UnitSelector selector, ConsumingMana consumingMana, bool kicker)
		{
			PlayBoard.UseCardRequestEntry useCardRequestEntry = new PlayBoard.UseCardRequestEntry(card, selector, consumingMana, kicker);
			return this.EnqueueRequest(useCardRequestEntry);
		}

		// Token: 0x0600093A RID: 2362 RVA: 0x0002F6E8 File Offset: 0x0002D8E8
		public void RequestUseUs(UnitSelector selector)
		{
			PlayBoard.UseUsRequestEntry useUsRequestEntry = new PlayBoard.UseUsRequestEntry(selector);
			this.EnqueueRequest(useUsRequestEntry);
		}

		// Token: 0x0600093B RID: 2363 RVA: 0x0002F704 File Offset: 0x0002D904
		public void RequestUseDoll(Doll doll, UnitSelector selector)
		{
			PlayBoard.UseDollRequestEntry useDollRequestEntry = new PlayBoard.UseDollRequestEntry(doll, selector);
			this.EnqueueRequest(useDollRequestEntry);
		}

		// Token: 0x0600093C RID: 2364 RVA: 0x0002F724 File Offset: 0x0002D924
		private void RequestEndTurn()
		{
			this.endTurnEdgeCg.alpha = 0f;
			this.endTurnButton.gameObject.SetActive(false);
			PlayBoard.EndTurnRequestEntry endTurnRequestEntry = new PlayBoard.EndTurnRequestEntry();
			this.EnqueueRequest(endTurnRequestEntry);
		}

		// Token: 0x0600093D RID: 2365 RVA: 0x0002F760 File Offset: 0x0002D960
		private bool EnqueueRequest(PlayBoard.RequestEntry request)
		{
			request.PlayBoard = this;
			if (base.Battle.IsWaitingPlayerInput)
			{
				if (!this._requests.Empty<PlayBoard.RequestEntry>())
				{
					Debug.LogWarning("Waiting player input while request quene is not empty");
				}
				if (!request.Verify(false))
				{
					return false;
				}
				request.Prepay();
				request.Commit();
			}
			else
			{
				if (!request.Verify(false))
				{
					return false;
				}
				request.Prepay();
				this._requests.Enqueue(request);
			}
			return true;
		}

		// Token: 0x0600093E RID: 2366 RVA: 0x0002F7D0 File Offset: 0x0002D9D0
		public void RewindRequests()
		{
			foreach (PlayBoard.RequestEntry requestEntry in Enumerable.Reverse<PlayBoard.RequestEntry>(this._requests))
			{
				requestEntry.Rewind();
			}
			this._requests.Clear();
		}

		// Token: 0x0600093F RID: 2367 RVA: 0x0002F82C File Offset: 0x0002DA2C
		private void CommitRequestHead()
		{
			if (this._requests.Empty<PlayBoard.RequestEntry>())
			{
				BattleHintPanel panel = UiManager.GetPanel<BattleHintPanel>();
				if (panel.IsVisible)
				{
					panel.StartBattleHint();
				}
				this.cardUi.RefreshAllCardsEdge();
				return;
			}
			PlayBoard.RequestEntry requestEntry = this._requests.Dequeue();
			if (requestEntry.Verify(true))
			{
				requestEntry.Commit();
				return;
			}
			this.RewindRequests();
			requestEntry.Rewind();
		}

		// Token: 0x06000940 RID: 2368 RVA: 0x0002F88E File Offset: 0x0002DA8E
		private void OnWaitingPlayerInput()
		{
			this.CommitRequestHead();
		}

		// Token: 0x06000942 RID: 2370 RVA: 0x0002F8C0 File Offset: 0x0002DAC0
		[CompilerGenerated]
		internal static bool <GetHoveringIndex>g__IsInCard|49_0(Vector2 pointerPosition, Vector2 cardRoot, float cardRotate)
		{
			float num;
			float num2;
			(pointerPosition - cardRoot).Deconstruct(out num, out num2);
			float num3 = num;
			float num4 = num2;
			float num5 = cardRotate.ToRadian();
			num2 = -Mathf.Sin(num5);
			float num6 = Mathf.Cos(num5);
			float num7 = num2;
			float num8 = num6;
			num2 = num3 * num8 - num4 * num7;
			float num9 = num3 * num7 + num4 * num8;
			num3 = num2;
			num4 = num9;
			return num3 > -185.5f && num3 < 185.5f && num4 > -259f && num4 < 259f;
		}

		// Token: 0x040006AC RID: 1708
		[SerializeField]
		private CardUi cardUi;

		// Token: 0x040006AD RID: 1709
		[SerializeField]
		private Button endTurnButton;

		// Token: 0x040006AE RID: 1710
		[SerializeField]
		private CanvasGroup endTurnEdgeCg;

		// Token: 0x040006AF RID: 1711
		[SerializeField]
		private TargetSelector targetSelector;

		// Token: 0x040006B0 RID: 1712
		[SerializeField]
		private RectTransform effectLayer;

		// Token: 0x040006B1 RID: 1713
		[SerializeField]
		private Transform manaConsumeEffect;

		// Token: 0x040006B2 RID: 1714
		[SerializeField]
		private Transform manaLoseEffect;

		// Token: 0x040006B3 RID: 1715
		[SerializeField]
		private ManaFlyEffect manaFlyEffect;

		// Token: 0x040006B4 RID: 1716
		private const float CardOriginWidth = 530f;

		// Token: 0x040006B5 RID: 1717
		private const float CardOriginHeight = 740f;

		// Token: 0x040006B6 RID: 1718
		private const float CardHalfWidth = 185.5f;

		// Token: 0x040006B7 RID: 1719
		private const float CardHalfHeight = 259f;

		// Token: 0x040006B8 RID: 1720
		private string _cardNotInHand;

		// Token: 0x040006B9 RID: 1721
		private string _cardCostChanged;

		// Token: 0x040006BA RID: 1722
		private string _usUsedThisBattle;

		// Token: 0x040006BB RID: 1723
		private string _usUsedThisTurn;

		// Token: 0x040006BC RID: 1724
		private string _targetAlreadyDead;

		// Token: 0x040006BD RID: 1725
		private string _moneyNotEnough;

		// Token: 0x040006BE RID: 1726
		private string _lowMana;

		// Token: 0x040006BF RID: 1727
		private string _lowPower;

		// Token: 0x040006C0 RID: 1728
		private string _lowMagic;

		// Token: 0x040006C1 RID: 1729
		private string _lowLoyalty;

		// Token: 0x040006C2 RID: 1730
		private string _handFull;

		// Token: 0x040006C3 RID: 1731
		private string _emptyDraw;

		// Token: 0x040006C4 RID: 1732
		private float _pointerX;

		// Token: 0x040006C5 RID: 1733
		private float _pointerY;

		// Token: 0x040006C6 RID: 1734
		private float _screenScale = 1f;

		// Token: 0x040006C7 RID: 1735
		private bool _pointered;

		// Token: 0x040006C8 RID: 1736
		private PlayBoard.InteractionStatus _status;

		// Token: 0x040006C9 RID: 1737
		private bool _pointerUpUse;

		// Token: 0x040006CA RID: 1738
		private int? _hoveringIndex;

		// Token: 0x040006CB RID: 1739
		private HandCard _activeHand;

		// Token: 0x040006CC RID: 1740
		private DollInfoWidget _activeDoll;

		// Token: 0x040006CD RID: 1741
		private bool _pointerInHandZone;

		// Token: 0x040006CE RID: 1742
		private bool _pointerInUseZone;

		// Token: 0x040006CF RID: 1743
		private int _playerContinousTurnCounter;

		// Token: 0x040006D2 RID: 1746
		private bool _isTempLockedFromMinimize;

		// Token: 0x040006D3 RID: 1747
		private IObjectPool<Transform> _manaConsumeEffectPool;

		// Token: 0x040006D4 RID: 1748
		private IObjectPool<Transform> _manaLoseEffectPool;

		// Token: 0x040006D5 RID: 1749
		private IObjectPool<ManaFlyEffect> _manaFlyEffectPool;

		// Token: 0x040006D6 RID: 1750
		private readonly PriorityQueue<Action, float> _effectTimeoutActionQueue = new PriorityQueue<Action, float>(null);

		// Token: 0x040006D7 RID: 1751
		private readonly Queue<PlayBoard.RequestEntry> _requests = new Queue<PlayBoard.RequestEntry>();

		// Token: 0x02000282 RID: 642
		public enum InteractionStatus
		{
			// Token: 0x04001158 RID: 4440
			Inactive,
			// Token: 0x04001159 RID: 4441
			Normal,
			// Token: 0x0400115A RID: 4442
			CardSelected,
			// Token: 0x0400115B RID: 4443
			CardTargetSelecting,
			// Token: 0x0400115C RID: 4444
			CardUsingConfirming,
			// Token: 0x0400115D RID: 4445
			UsTargetSelecting,
			// Token: 0x0400115E RID: 4446
			DollTargetSelecting
		}

		// Token: 0x02000283 RID: 643
		private abstract class RequestEntry
		{
			// Token: 0x1700044D RID: 1101
			// (get) Token: 0x060015E6 RID: 5606 RVA: 0x000637BC File Offset: 0x000619BC
			// (set) Token: 0x060015E7 RID: 5607 RVA: 0x000637DB File Offset: 0x000619DB
			public PlayBoard PlayBoard
			{
				get
				{
					PlayBoard playBoard;
					if (!this._playBoard.TryGetTarget(ref playBoard))
					{
						return null;
					}
					return playBoard;
				}
				set
				{
					this._playBoard.SetTarget(value);
				}
			}

			// Token: 0x060015E8 RID: 5608
			public abstract bool Verify(bool isCommitting);

			// Token: 0x060015E9 RID: 5609
			public abstract void Prepay();

			// Token: 0x060015EA RID: 5610
			public abstract void Commit();

			// Token: 0x060015EB RID: 5611
			public abstract void Rewind();

			// Token: 0x0400115F RID: 4447
			private readonly WeakReference<PlayBoard> _playBoard = new WeakReference<PlayBoard>(null);
		}

		// Token: 0x02000284 RID: 644
		private sealed class UseCardRequestEntry : PlayBoard.RequestEntry
		{
			// Token: 0x060015ED RID: 5613 RVA: 0x00063800 File Offset: 0x00061A00
			public UseCardRequestEntry(Card card, UnitSelector selector, ConsumingMana consumingMana, bool kicker)
			{
				ManaGroup cost = card.Cost;
				this._card = card;
				this._selector = selector;
				this._consumingMana = consumingMana;
				this._kicker = kicker;
				this._originalCost = cost;
			}

			// Token: 0x060015EE RID: 5614 RVA: 0x00063848 File Offset: 0x00061A48
			public override bool Verify(bool isCommitting)
			{
				if (isCommitting && this._originalCost != this._card.Cost)
				{
					base.PlayBoard.ShowErrorChat(base.PlayBoard._cardCostChanged);
					return false;
				}
				return base.PlayBoard.UseCardVerify(this._card, this._selector, this._consumingMana, isCommitting);
			}

			// Token: 0x060015EF RID: 5615 RVA: 0x000638A6 File Offset: 0x00061AA6
			public override void Prepay()
			{
				UiManager.GetPanel<BattleManaPanel>().Prepay(this._consumingMana);
			}

			// Token: 0x060015F0 RID: 5616 RVA: 0x000638B8 File Offset: 0x00061AB8
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestUseCard(this._card, this._selector, this._consumingMana.TotalMana, this._kicker);
			}

			// Token: 0x060015F1 RID: 5617 RVA: 0x000638E7 File Offset: 0x00061AE7
			public override void Rewind()
			{
				Debug.LogWarning(string.Format("Rewind card: '{0}' -> {1}", this._card.DebugName, this._selector));
				base.PlayBoard.RewindCard(this._card, this._consumingMana);
			}

			// Token: 0x060015F2 RID: 5618 RVA: 0x00063920 File Offset: 0x00061B20
			public override string ToString()
			{
				return string.Format("UseCard: {0} --{1}--> {2}", this._card.DebugName, this._consumingMana, this._selector);
			}

			// Token: 0x04001160 RID: 4448
			private readonly Card _card;

			// Token: 0x04001161 RID: 4449
			private readonly UnitSelector _selector;

			// Token: 0x04001162 RID: 4450
			private readonly ConsumingMana _consumingMana;

			// Token: 0x04001163 RID: 4451
			private readonly bool _kicker;

			// Token: 0x04001164 RID: 4452
			private readonly ManaGroup _originalCost;
		}

		// Token: 0x02000285 RID: 645
		private sealed class UseUsRequestEntry : PlayBoard.RequestEntry
		{
			// Token: 0x060015F3 RID: 5619 RVA: 0x00063943 File Offset: 0x00061B43
			public UseUsRequestEntry(UnitSelector selector)
			{
				this._selector = selector;
			}

			// Token: 0x060015F4 RID: 5620 RVA: 0x00063952 File Offset: 0x00061B52
			public override bool Verify(bool isCommitting)
			{
				return base.PlayBoard.UseUsVerify(this._selector);
			}

			// Token: 0x060015F5 RID: 5621 RVA: 0x00063965 File Offset: 0x00061B65
			public override void Prepay()
			{
			}

			// Token: 0x060015F6 RID: 5622 RVA: 0x00063967 File Offset: 0x00061B67
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestUseUs(this._selector);
			}

			// Token: 0x060015F7 RID: 5623 RVA: 0x0006397F File Offset: 0x00061B7F
			public override void Rewind()
			{
				Debug.LogWarning(string.Format("Rewind US -> {0}", this._selector));
				base.PlayBoard.RewindUs();
			}

			// Token: 0x060015F8 RID: 5624 RVA: 0x000639A1 File Offset: 0x00061BA1
			public override string ToString()
			{
				return string.Format("UseUS: {0}", this._selector);
			}

			// Token: 0x04001165 RID: 4453
			private readonly UnitSelector _selector;
		}

		// Token: 0x02000286 RID: 646
		private sealed class UseDollRequestEntry : PlayBoard.RequestEntry
		{
			// Token: 0x060015F9 RID: 5625 RVA: 0x000639B4 File Offset: 0x00061BB4
			public UseDollRequestEntry(Doll doll, UnitSelector selector)
			{
				this._doll = doll;
				this._selector = selector;
			}

			// Token: 0x060015FA RID: 5626 RVA: 0x000639D9 File Offset: 0x00061BD9
			public override bool Verify(bool isCommitting)
			{
				return base.PlayBoard.UseDollVerify(this._doll, this._selector);
			}

			// Token: 0x060015FB RID: 5627 RVA: 0x000639F2 File Offset: 0x00061BF2
			public override void Prepay()
			{
			}

			// Token: 0x060015FC RID: 5628 RVA: 0x000639F4 File Offset: 0x00061BF4
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestUseDoll(this._doll, this._selector);
			}

			// Token: 0x060015FD RID: 5629 RVA: 0x00063A12 File Offset: 0x00061C12
			public override void Rewind()
			{
				Debug.LogWarning(string.Format("Rewind doll: '{0}' -> {1}", this._doll.DebugName, this._selector));
				base.PlayBoard.RewindDoll(this._doll);
			}

			// Token: 0x060015FE RID: 5630 RVA: 0x00063A45 File Offset: 0x00061C45
			public override string ToString()
			{
				return string.Format("UseDoll: {0} -> {1}", this._doll.DebugName, this._selector);
			}

			// Token: 0x04001166 RID: 4454
			private readonly Doll _doll;

			// Token: 0x04001167 RID: 4455
			private readonly UnitSelector _selector;
		}

		// Token: 0x02000287 RID: 647
		private sealed class EndTurnRequestEntry : PlayBoard.RequestEntry
		{
			// Token: 0x060015FF RID: 5631 RVA: 0x00063A62 File Offset: 0x00061C62
			public override bool Verify(bool isCommitting)
			{
				return true;
			}

			// Token: 0x06001600 RID: 5632 RVA: 0x00063A65 File Offset: 0x00061C65
			public override void Prepay()
			{
			}

			// Token: 0x06001601 RID: 5633 RVA: 0x00063A67 File Offset: 0x00061C67
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestEndPlayerTurn();
			}

			// Token: 0x06001602 RID: 5634 RVA: 0x00063A79 File Offset: 0x00061C79
			public override void Rewind()
			{
				Debug.LogWarning("Rewind end-turn");
				base.PlayBoard.endTurnButton.gameObject.SetActive(true);
			}

			// Token: 0x06001603 RID: 5635 RVA: 0x00063A9B File Offset: 0x00061C9B
			public override string ToString()
			{
				return "EndTurn";
			}
		}
	}
}
