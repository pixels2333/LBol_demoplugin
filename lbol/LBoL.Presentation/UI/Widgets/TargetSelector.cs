using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.Units;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000077 RID: 119
	public class TargetSelector : MonoBehaviour
	{
		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000619 RID: 1561 RVA: 0x0001A4DB File Offset: 0x000186DB
		// (set) Token: 0x0600061A RID: 1562 RVA: 0x0001A4E3 File Offset: 0x000186E3
		public bool PlayBoardHasPointer { get; set; }

		// Token: 0x0600061B RID: 1563 RVA: 0x0001A4EC File Offset: 0x000186EC
		private void Awake()
		{
			this._parent = (RectTransform)base.transform.parent;
			this.root.gameObject.SetActive(false);
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x0001A515 File Offset: 0x00018715
		private void OnEnable()
		{
			this._singleEnemyAutoSelect = GameMaster.SingleEnemyAutoSelect;
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.OnSettingChanged);
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x0001A533 File Offset: 0x00018733
		private void OnDisable()
		{
			GameMaster.SettingsChanged -= new Action<GameSettingsSaveData>(this.OnSettingChanged);
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x0001A546 File Offset: 0x00018746
		private void OnSettingChanged(GameSettingsSaveData data)
		{
			this._singleEnemyAutoSelect = data.SingleEnemyAutoSelect;
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x0001A554 File Offset: 0x00018754
		private void Update()
		{
			if (this._status == TargetSelectorStatus.None || !this.PlayBoardHasPointer)
			{
				return;
			}
			this.UpdateMousePosition(false);
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x0001A570 File Offset: 0x00018770
		private void UpdateMousePosition(bool forced)
		{
			Mouse current = Mouse.current;
			if (current != null)
			{
				Vector2 vector = current.position.ReadValue();
				if (vector != Vector2.zero)
				{
					RectTransformUtility.ScreenPointToLocalPointInRectangle(this._parent, vector, CameraController.UiCamera, out this._currentMousePosition);
					if (forced || this._currentMousePosition != this._prevMousePosition)
					{
						this.UpdatePointerPosition(this._currentMousePosition);
						this._prevMousePosition = this._currentMousePosition;
					}
				}
			}
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x0001A5E5 File Offset: 0x000187E5
		public void ForceUpdate()
		{
			if (this._status == TargetSelectorStatus.None)
			{
				return;
			}
			this.UpdateMousePosition(true);
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x0001A5F8 File Offset: 0x000187F8
		private void UpdatePointerPosition(Vector2 position)
		{
			if (this._targetSelecting && this._targetType == TargetType.SingleEnemy)
			{
				this.UpdateSingleEnemy();
				this.SetCurveLine(this._arrowTailPosition, position);
				return;
			}
			this._activeHand.ActiveHandPosition = position;
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x0001A648 File Offset: 0x00018848
		private void UpdateSingleEnemy()
		{
			Unit unit = null;
			if (this._singleEnemyAutoSelect && this._potentialTargets.Count == 1)
			{
				UnitView unitView = this._potentialTargets[0];
				unitView.SelectingVisible = true;
				unit = unitView.Unit;
			}
			else
			{
				Ray ray = CameraController.MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
				bool flag = false;
				foreach (UnitView unitView2 in this._potentialTargets)
				{
					if (!flag && unitView2.SelectorCollider)
					{
						RaycastHit raycastHit;
						flag = unitView2.SelectorCollider.Raycast(ray, out raycastHit, float.PositiveInfinity);
						unitView2.SelectingVisible = flag;
						if (flag)
						{
							unit = unitView2.Unit;
						}
					}
					else
					{
						unitView2.SelectingVisible = false;
					}
				}
			}
			if (this._activeHand)
			{
				this._activeHand.Card.PendingTarget = unit;
				return;
			}
			if (this._activeUs != null)
			{
				this._activeUs.PendingTarget = unit;
			}
		}

		// Token: 0x06000624 RID: 1572 RVA: 0x0001A764 File Offset: 0x00018964
		private EnemyUnit GetPointedEnemy(Vector2 screenPosition)
		{
			if (this._singleEnemyAutoSelect && this._potentialTargets.Count == 1)
			{
				EnemyUnit enemyUnit = this._potentialTargets[0].Unit as EnemyUnit;
				if (enemyUnit != null)
				{
					return enemyUnit;
				}
			}
			Ray ray = CameraController.MainCamera.ScreenPointToRay(screenPosition);
			foreach (UnitView unitView in this._potentialTargets)
			{
				RaycastHit raycastHit;
				if (unitView.SelectorCollider != null && unitView.SelectorCollider.Raycast(ray, out raycastHit, float.PositiveInfinity))
				{
					return (EnemyUnit)unitView.Unit;
				}
			}
			return null;
		}

		// Token: 0x06000625 RID: 1573 RVA: 0x0001A82C File Offset: 0x00018A2C
		private void SetCurveLine(Vector3 startPosition, Vector3 endPosition)
		{
			this.curveLine.SetLine(startPosition, endPosition);
		}

		// Token: 0x06000626 RID: 1574 RVA: 0x0001A83B File Offset: 0x00018A3B
		private void SetPotentialTargets()
		{
			this._potentialTargets.AddRange(GameDirector.EnumeratePotentialTargets(this._targetType));
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x0001A854 File Offset: 0x00018A54
		private void ClearPotentialTargets()
		{
			foreach (UnitView unitView in this._potentialTargets)
			{
				unitView.SelectingVisible = false;
			}
			this._potentialTargets.Clear();
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x0001A8B0 File Offset: 0x00018AB0
		public void EnableSelector(HandCard hand)
		{
			if (hand == null || hand.Card == null)
			{
				throw new ArgumentNullException("hand");
			}
			this._activeHand = hand;
			this._targetType = this._activeHand.Card.Config.TargetType.Value;
			this._arrowTailPosition = this.cardUsingPosition.localPosition;
			this.SetPotentialTargets();
			hand.ActiveHandRotation = Quaternion.identity;
			this.root.gameObject.SetActive(true);
			this._status = TargetSelectorStatus.Card;
			this._targetSelecting = false;
			this.UpdateMousePosition(false);
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x0001A950 File Offset: 0x00018B50
		public void EnableSelector(UltimateSkill us, Vector3 fromWorldPosition)
		{
			if (us.TargetType != TargetType.SingleEnemy)
			{
				throw new InvalidOperationException(string.Format("Cannot enable selector for US type {0}", us.TargetType));
			}
			this._activeUs = us;
			this._targetType = us.TargetType;
			this._arrowTailPosition = base.transform.InverseTransformPoint(fromWorldPosition);
			this.SetPotentialTargets();
			this.curveLine.gameObject.SetActive(true);
			this._hidingCursor = true;
			Cursor.visible = false;
			this.root.gameObject.SetActive(true);
			this._status = TargetSelectorStatus.Us;
			this._targetSelecting = true;
			this.UpdateMousePosition(true);
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x0001A9F8 File Offset: 0x00018BF8
		public void EnableSelector(Doll doll, Vector3 fromWorldPosition)
		{
			if (doll.TargetType != TargetType.SingleEnemy)
			{
				throw new InvalidOperationException(string.Format("Cannot enable selector for doll type {0}", doll.TargetType));
			}
			this._activeDoll = doll;
			this._targetType = doll.TargetType;
			this._arrowTailPosition = base.transform.InverseTransformPoint(fromWorldPosition);
			this.SetPotentialTargets();
			this.curveLine.gameObject.SetActive(true);
			this._hidingCursor = true;
			Cursor.visible = false;
			this.root.gameObject.SetActive(true);
			this._status = TargetSelectorStatus.Doll;
			this._targetSelecting = true;
			this.UpdateMousePosition(true);
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x0001AAA0 File Offset: 0x00018CA0
		public void EnterUseZone(HandCard hand)
		{
			this._targetSelecting = true;
			TargetType? targetType = hand.Card.Config.TargetType;
			TargetType targetType2 = TargetType.SingleEnemy;
			if ((targetType.GetValueOrDefault() == targetType2) & (targetType != null))
			{
				this.curveLine.gameObject.SetActive(true);
				this._hidingCursor = true;
				Cursor.visible = false;
				hand.ActiveHandPosition = this.cardUsingPosition.localPosition;
			}
			else
			{
				this.curveLine.gameObject.SetActive(false);
				this._hidingCursor = false;
				Cursor.visible = true;
			}
			targetType2 = this._targetType;
			if (targetType2 == TargetType.All || targetType2 == TargetType.AllEnemies || targetType2 == TargetType.RandomEnemy || targetType2 == TargetType.Self)
			{
				foreach (UnitView unitView in this._potentialTargets)
				{
					unitView.SelectingVisible = true;
				}
			}
		}

		// Token: 0x0600062C RID: 1580 RVA: 0x0001AB88 File Offset: 0x00018D88
		public void LeaveUseZone()
		{
			TargetType targetType = this._targetType;
			if (targetType == TargetType.All || targetType == TargetType.AllEnemies || targetType == TargetType.RandomEnemy || targetType == TargetType.Self)
			{
				foreach (UnitView unitView in this._potentialTargets)
				{
					unitView.SelectingVisible = false;
				}
			}
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0001ABF0 File Offset: 0x00018DF0
		public void DisableSelector()
		{
			this.root.gameObject.SetActive(false);
			this.curveLine.gameObject.SetActive(false);
			this._hidingCursor = false;
			Cursor.visible = true;
			this._targetSelecting = false;
			if (this._status == TargetSelectorStatus.Card)
			{
				this._activeHand.Card.PendingTarget = null;
				this._activeHand.Card.PendingManaUsage = default(ManaGroup?);
				this._activeHand = null;
			}
			else if (this._status == TargetSelectorStatus.Us)
			{
				this._activeUs.PendingTarget = null;
				this._activeUs = null;
			}
			this.ClearPotentialTargets();
			this._status = TargetSelectorStatus.None;
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x0001AC98 File Offset: 0x00018E98
		[return: MaybeNull]
		public UnitSelector GetConfirmUseSelector(Vector2 screenPosition)
		{
			UnitSelector unitSelector = null;
			switch (this._targetType)
			{
			case TargetType.Nobody:
				unitSelector = UnitSelector.Nobody;
				break;
			case TargetType.SingleEnemy:
			{
				EnemyUnit pointedEnemy = this.GetPointedEnemy(screenPosition);
				if (pointedEnemy != null)
				{
					unitSelector = new UnitSelector(pointedEnemy);
				}
				break;
			}
			case TargetType.AllEnemies:
				unitSelector = UnitSelector.AllEnemies;
				break;
			case TargetType.RandomEnemy:
				unitSelector = UnitSelector.RandomEnemy;
				break;
			case TargetType.Self:
				unitSelector = UnitSelector.Self;
				break;
			case TargetType.All:
				unitSelector = UnitSelector.All;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return unitSelector;
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0001AD11 File Offset: 0x00018F11
		public void SetCursorVisible()
		{
			Cursor.visible = !this._hidingCursor;
		}

		// Token: 0x040003C0 RID: 960
		[SerializeField]
		private Transform cardUsingPosition;

		// Token: 0x040003C1 RID: 961
		[SerializeField]
		private RectTransform root;

		// Token: 0x040003C2 RID: 962
		[SerializeField]
		private CurveLine curveLine;

		// Token: 0x040003C3 RID: 963
		private RectTransform _parent;

		// Token: 0x040003C4 RID: 964
		private TargetType _targetType;

		// Token: 0x040003C5 RID: 965
		private HandCard _activeHand;

		// Token: 0x040003C6 RID: 966
		private UltimateSkill _activeUs;

		// Token: 0x040003C7 RID: 967
		private Doll _activeDoll;

		// Token: 0x040003C8 RID: 968
		private TargetSelectorStatus _status;

		// Token: 0x040003C9 RID: 969
		private bool _targetSelecting;

		// Token: 0x040003CA RID: 970
		private bool _hidingCursor;

		// Token: 0x040003CB RID: 971
		private Vector2 _arrowTailPosition;

		// Token: 0x040003CC RID: 972
		private Vector2 _prevMousePosition = Vector2.zero;

		// Token: 0x040003CD RID: 973
		private Vector2 _currentMousePosition = Vector2.zero;

		// Token: 0x040003CE RID: 974
		private readonly List<UnitView> _potentialTargets = new List<UnitView>();

		// Token: 0x040003CF RID: 975
		private bool _singleEnemyAutoSelect;
	}
}
