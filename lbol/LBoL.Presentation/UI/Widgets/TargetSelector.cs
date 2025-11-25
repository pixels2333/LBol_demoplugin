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
	public class TargetSelector : MonoBehaviour
	{
		public bool PlayBoardHasPointer { get; set; }
		private void Awake()
		{
			this._parent = (RectTransform)base.transform.parent;
			this.root.gameObject.SetActive(false);
		}
		private void OnEnable()
		{
			this._singleEnemyAutoSelect = GameMaster.SingleEnemyAutoSelect;
			GameMaster.SettingsChanged += new Action<GameSettingsSaveData>(this.OnSettingChanged);
		}
		private void OnDisable()
		{
			GameMaster.SettingsChanged -= new Action<GameSettingsSaveData>(this.OnSettingChanged);
		}
		private void OnSettingChanged(GameSettingsSaveData data)
		{
			this._singleEnemyAutoSelect = data.SingleEnemyAutoSelect;
		}
		private void Update()
		{
			if (this._status == TargetSelectorStatus.None || !this.PlayBoardHasPointer)
			{
				return;
			}
			this.UpdateMousePosition(false);
		}
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
		public void ForceUpdate()
		{
			if (this._status == TargetSelectorStatus.None)
			{
				return;
			}
			this.UpdateMousePosition(true);
		}
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
		private void SetCurveLine(Vector3 startPosition, Vector3 endPosition)
		{
			this.curveLine.SetLine(startPosition, endPosition);
		}
		private void SetPotentialTargets()
		{
			this._potentialTargets.AddRange(GameDirector.EnumeratePotentialTargets(this._targetType));
		}
		private void ClearPotentialTargets()
		{
			foreach (UnitView unitView in this._potentialTargets)
			{
				unitView.SelectingVisible = false;
			}
			this._potentialTargets.Clear();
		}
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
		public void SetCursorVisible()
		{
			Cursor.visible = !this._hidingCursor;
		}
		[SerializeField]
		private Transform cardUsingPosition;
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private CurveLine curveLine;
		private RectTransform _parent;
		private TargetType _targetType;
		private HandCard _activeHand;
		private UltimateSkill _activeUs;
		private Doll _activeDoll;
		private TargetSelectorStatus _status;
		private bool _targetSelecting;
		private bool _hidingCursor;
		private Vector2 _arrowTailPosition;
		private Vector2 _prevMousePosition = Vector2.zero;
		private Vector2 _currentMousePosition = Vector2.zero;
		private readonly List<UnitView> _potentialTargets = new List<UnitView>();
		private bool _singleEnemyAutoSelect;
	}
}
