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
	public sealed class PlayBoard : UiPanel, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Base;
			}
		}
		public CardUi CardUi
		{
			get
			{
				return this.cardUi;
			}
		}
		public Transform ActiveHandParent
		{
			get
			{
				return this.targetSelector.transform;
			}
		}
		public void Awake()
		{
			this.endTurnButton.onClick.AddListener(new UnityAction(this.UI_EndTurn));
			SimpleTooltipSource.CreateWithTooltipKeyAndArgs(this.endTurnButton.gameObject, "EndTurn", new object[] { 5 }).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.endTurnEdgeCg.alpha = 0f;
			this.endTurnButton.gameObject.SetActive(false);
			this.InitEffects();
		}
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
		public void ReverifyCard()
		{
			bool flag;
			if (this._status == PlayBoard.InteractionStatus.CardTargetSelecting && !this.UseCardVerify(this._activeHand.Card, true, out flag))
			{
				this.CancelTargetSelecting(true);
			}
		}
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
		public void OnPointerEnter(PointerEventData eventData)
		{
			this._pointered = true;
			this.targetSelector.PlayBoardHasPointer = true;
		}
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
		public bool HandleConfirmAction()
		{
			BattleController battle = base.Battle;
			return false;
		}
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
		public void CancelTargetSelectingIfCardIs(HandCard hand)
		{
			if (hand == this._activeHand)
			{
				Debug.Log("[PlayBoard] Cancel target selecting because hand is " + hand.Card.DebugName + ", maybe moving/discarding/exiling");
				this.CancelTargetSelecting(true);
			}
		}
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
		private void OnGlobalStatusChanged()
		{
			this.cardUi.RefreshAll();
		}
		private bool PlayerInTurn { get; set; }
		private Coroutine PlayerInTurnSetter { get; set; }
		private IEnumerator ViewStartBattle(StartBattleAction action)
		{
			UiManager.GetPanel<SystemBoard>().SetBattleStatus(BattleStatus.BattleStart);
			yield return UiManager.GetPanel<BattleNotifier>().ShowBattleStart();
			GameDirector.FadeInEnemyStatus();
			this._playerContinousTurnCounter = 0;
			yield break;
		}
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
		private IEnumerator PlayerInTurnRunner()
		{
			yield return new WaitForSecondsRealtime(0.3f);
			this.PlayerInTurn = true;
			yield break;
		}
		private IEnumerator ViewStartAllEnemyTurn(StartAllEnemyTurnAction action)
		{
			UiManager.GetPanel<SystemBoard>().SetBattleStatus(BattleStatus.EnemyTurn);
			yield return UiManager.GetPanel<BattleNotifier>().ShowEnemyTurn(base.Battle.RoundCounter);
			yield break;
		}
		private IEnumerator ViewEndBattle(EndBattleAction action)
		{
			UiManager.GetPanel<SystemBoard>().SetBattleStatus(BattleStatus.OutOfBattle);
			yield break;
		}
		private IEnumerator ViewStartEnemyTurn(StartEnemyTurnAction action)
		{
			if (action.Unit.IsExtraTurn)
			{
				Debug.Log(string.Format("View {0} extra-turn (turn {1})", action.Unit.DebugName, action.Unit.TurnCounter));
			}
			yield break;
		}
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
		private void UI_EndTurn()
		{
			if (!this.IsTempLockedFromMinimize)
			{
				this.RequestEndTurn();
			}
		}
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
		private void RewindCard(Card card, ConsumingMana consumingMana)
		{
			this.cardUi.CancelUse(card);
			UiManager.GetPanel<BattleManaPanel>().RefundBack(consumingMana);
		}
		private void RewindUs()
		{
			UiManager.GetPanel<UltimateSkillPanel>().CancelUse();
		}
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
		public void SetEndTurnParticle(bool active)
		{
			if (!base.GameRun.Battle.IsWaitingPlayerInput)
			{
				return;
			}
			this.endTurnEdgeCg.DOFade((float)(active ? 1 : 0), 0.33f).SetLink(this.endTurnEdgeCg.gameObject);
		}
		private void ShowErrorChat(string info)
		{
			GameDirector.Player.Chat(info, 3f, ChatWidget.CloudType.LeftThink, 0f);
		}
		public void ShowMoneyNotEnough()
		{
			this.ShowErrorChat(this._moneyNotEnough);
		}
		public void ShowLowMana()
		{
			this.ShowErrorChat(this._lowMana);
		}
		public void ShowLowPower()
		{
			this.ShowErrorChat(this._lowPower);
		}
		public void ShowLowMagic()
		{
			this.ShowErrorChat(this._lowMagic);
		}
		public void ShowLowLoyalty()
		{
			this.ShowErrorChat(this._lowLoyalty);
		}
		public void ShowUsUsedThisTurn()
		{
			this.ShowErrorChat(this._usUsedThisTurn);
		}
		public void ShowUsUsedThisBattle()
		{
			this.ShowErrorChat(this._usUsedThisBattle);
		}
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
		public void SetCursorVisible()
		{
			this.targetSelector.SetCursorVisible();
		}
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
		private bool RequestUseCard(Card card, UnitSelector selector, ConsumingMana consumingMana, bool kicker)
		{
			PlayBoard.UseCardRequestEntry useCardRequestEntry = new PlayBoard.UseCardRequestEntry(card, selector, consumingMana, kicker);
			return this.EnqueueRequest(useCardRequestEntry);
		}
		public void RequestUseUs(UnitSelector selector)
		{
			PlayBoard.UseUsRequestEntry useUsRequestEntry = new PlayBoard.UseUsRequestEntry(selector);
			this.EnqueueRequest(useUsRequestEntry);
		}
		public void RequestUseDoll(Doll doll, UnitSelector selector)
		{
			PlayBoard.UseDollRequestEntry useDollRequestEntry = new PlayBoard.UseDollRequestEntry(doll, selector);
			this.EnqueueRequest(useDollRequestEntry);
		}
		private void RequestEndTurn()
		{
			this.endTurnEdgeCg.alpha = 0f;
			this.endTurnButton.gameObject.SetActive(false);
			PlayBoard.EndTurnRequestEntry endTurnRequestEntry = new PlayBoard.EndTurnRequestEntry();
			this.EnqueueRequest(endTurnRequestEntry);
		}
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
		public void RewindRequests()
		{
			foreach (PlayBoard.RequestEntry requestEntry in Enumerable.Reverse<PlayBoard.RequestEntry>(this._requests))
			{
				requestEntry.Rewind();
			}
			this._requests.Clear();
		}
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
		private void OnWaitingPlayerInput()
		{
			this.CommitRequestHead();
		}
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
		[SerializeField]
		private CardUi cardUi;
		[SerializeField]
		private Button endTurnButton;
		[SerializeField]
		private CanvasGroup endTurnEdgeCg;
		[SerializeField]
		private TargetSelector targetSelector;
		[SerializeField]
		private RectTransform effectLayer;
		[SerializeField]
		private Transform manaConsumeEffect;
		[SerializeField]
		private Transform manaLoseEffect;
		[SerializeField]
		private ManaFlyEffect manaFlyEffect;
		private const float CardOriginWidth = 530f;
		private const float CardOriginHeight = 740f;
		private const float CardHalfWidth = 185.5f;
		private const float CardHalfHeight = 259f;
		private string _cardNotInHand;
		private string _cardCostChanged;
		private string _usUsedThisBattle;
		private string _usUsedThisTurn;
		private string _targetAlreadyDead;
		private string _moneyNotEnough;
		private string _lowMana;
		private string _lowPower;
		private string _lowMagic;
		private string _lowLoyalty;
		private string _handFull;
		private string _emptyDraw;
		private float _pointerX;
		private float _pointerY;
		private float _screenScale = 1f;
		private bool _pointered;
		private PlayBoard.InteractionStatus _status;
		private bool _pointerUpUse;
		private int? _hoveringIndex;
		private HandCard _activeHand;
		private DollInfoWidget _activeDoll;
		private bool _pointerInHandZone;
		private bool _pointerInUseZone;
		private int _playerContinousTurnCounter;
		private bool _isTempLockedFromMinimize;
		private IObjectPool<Transform> _manaConsumeEffectPool;
		private IObjectPool<Transform> _manaLoseEffectPool;
		private IObjectPool<ManaFlyEffect> _manaFlyEffectPool;
		private readonly PriorityQueue<Action, float> _effectTimeoutActionQueue = new PriorityQueue<Action, float>(null);
		private readonly Queue<PlayBoard.RequestEntry> _requests = new Queue<PlayBoard.RequestEntry>();
		public enum InteractionStatus
		{
			Inactive,
			Normal,
			CardSelected,
			CardTargetSelecting,
			CardUsingConfirming,
			UsTargetSelecting,
			DollTargetSelecting
		}
		private abstract class RequestEntry
		{
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
			public abstract bool Verify(bool isCommitting);
			public abstract void Prepay();
			public abstract void Commit();
			public abstract void Rewind();
			private readonly WeakReference<PlayBoard> _playBoard = new WeakReference<PlayBoard>(null);
		}
		private sealed class UseCardRequestEntry : PlayBoard.RequestEntry
		{
			public UseCardRequestEntry(Card card, UnitSelector selector, ConsumingMana consumingMana, bool kicker)
			{
				ManaGroup cost = card.Cost;
				this._card = card;
				this._selector = selector;
				this._consumingMana = consumingMana;
				this._kicker = kicker;
				this._originalCost = cost;
			}
			public override bool Verify(bool isCommitting)
			{
				if (isCommitting && this._originalCost != this._card.Cost)
				{
					base.PlayBoard.ShowErrorChat(base.PlayBoard._cardCostChanged);
					return false;
				}
				return base.PlayBoard.UseCardVerify(this._card, this._selector, this._consumingMana, isCommitting);
			}
			public override void Prepay()
			{
				UiManager.GetPanel<BattleManaPanel>().Prepay(this._consumingMana);
			}
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestUseCard(this._card, this._selector, this._consumingMana.TotalMana, this._kicker);
			}
			public override void Rewind()
			{
				Debug.LogWarning(string.Format("Rewind card: '{0}' -> {1}", this._card.DebugName, this._selector));
				base.PlayBoard.RewindCard(this._card, this._consumingMana);
			}
			public override string ToString()
			{
				return string.Format("UseCard: {0} --{1}--> {2}", this._card.DebugName, this._consumingMana, this._selector);
			}
			private readonly Card _card;
			private readonly UnitSelector _selector;
			private readonly ConsumingMana _consumingMana;
			private readonly bool _kicker;
			private readonly ManaGroup _originalCost;
		}
		private sealed class UseUsRequestEntry : PlayBoard.RequestEntry
		{
			public UseUsRequestEntry(UnitSelector selector)
			{
				this._selector = selector;
			}
			public override bool Verify(bool isCommitting)
			{
				return base.PlayBoard.UseUsVerify(this._selector);
			}
			public override void Prepay()
			{
			}
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestUseUs(this._selector);
			}
			public override void Rewind()
			{
				Debug.LogWarning(string.Format("Rewind US -> {0}", this._selector));
				base.PlayBoard.RewindUs();
			}
			public override string ToString()
			{
				return string.Format("UseUS: {0}", this._selector);
			}
			private readonly UnitSelector _selector;
		}
		private sealed class UseDollRequestEntry : PlayBoard.RequestEntry
		{
			public UseDollRequestEntry(Doll doll, UnitSelector selector)
			{
				this._doll = doll;
				this._selector = selector;
			}
			public override bool Verify(bool isCommitting)
			{
				return base.PlayBoard.UseDollVerify(this._doll, this._selector);
			}
			public override void Prepay()
			{
			}
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestUseDoll(this._doll, this._selector);
			}
			public override void Rewind()
			{
				Debug.LogWarning(string.Format("Rewind doll: '{0}' -> {1}", this._doll.DebugName, this._selector));
				base.PlayBoard.RewindDoll(this._doll);
			}
			public override string ToString()
			{
				return string.Format("UseDoll: {0} -> {1}", this._doll.DebugName, this._selector);
			}
			private readonly Doll _doll;
			private readonly UnitSelector _selector;
		}
		private sealed class EndTurnRequestEntry : PlayBoard.RequestEntry
		{
			public override bool Verify(bool isCommitting)
			{
				return true;
			}
			public override void Prepay()
			{
			}
			public override void Commit()
			{
				base.PlayBoard.Battle.RequestEndPlayerTurn();
			}
			public override void Rewind()
			{
				Debug.LogWarning("Rewind end-turn");
				base.PlayBoard.endTurnButton.gameObject.SetActive(true);
			}
			public override string ToString()
			{
				return "EndTurn";
			}
		}
	}
}
