using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LBoL.Base.Extensions;
using LBoL.Core.Battle.BattleActionRecord;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace LBoL.Presentation.UI.Panels
{
	public class DebugBattleLogPanel : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Topmost;
			}
		}
		private void Awake()
		{
			this.actionTemplate.gameObject.SetActive(false);
			this.actionHeaderTemplate.gameObject.SetActive(false);
		}
		public override UniTask InitializeAsync()
		{
			InputAction battleLog = UiManager.Instance.BattleLog;
			battleLog.performed += new Action<InputAction.CallbackContext>(this.OnDebugBattleLogClicked);
			this.infoText.text = "Battle Log (press " + battleLog.GetBindingDisplayString((InputBinding.DisplayStringOptions)0, null) + " to close)";
			this.SetActionButtons(ActionRecord.ActionRecords);
			ActionRecord.ActionResolvedHandler = (Action<ActionRecord>)Delegate.Combine(ActionRecord.ActionResolvedHandler, new Action<ActionRecord>(this.OnActionResolved));
			return UniTask.CompletedTask;
		}
		private void OnDestroy()
		{
			UiManager.Instance.BattleLog.performed -= new Action<InputAction.CallbackContext>(this.OnDebugBattleLogClicked);
			ActionRecord.ActionResolvedHandler = (Action<ActionRecord>)Delegate.Remove(ActionRecord.ActionResolvedHandler, new Action<ActionRecord>(this.OnActionResolved));
		}
		protected override void OnEnterBattle()
		{
			this.SetActionButtons(Enumerable.Empty<ActionRecord>());
		}
		protected override void OnLeaveBattle()
		{
		}
		protected override void OnShowing()
		{
			this.actionListScrollRect.ScrollToBottom();
			if (ActionRecord.ActionRecords.Count > 0)
			{
				this.SetGraph(Enumerable.Last<ActionRecord>(ActionRecord.ActionRecords));
			}
		}
		private void SetActionButtons(IEnumerable<ActionRecord> actionRecords)
		{
			this.actionListParent.DestroyChildren();
			ActionRecord actionRecord = null;
			foreach (ActionRecord actionRecord2 in actionRecords)
			{
				actionRecord = actionRecord2;
				this.AppendActionButton(actionRecord2);
			}
			if (actionRecord != null)
			{
				this.SetGraph(actionRecord);
				return;
			}
			this.ClearGraph();
		}
		private void OnActionResolved(ActionRecord actionRecord)
		{
			this.AppendActionButton(actionRecord);
			this.SetGraph(actionRecord);
			this.actionListScrollRect.ScrollToBottom();
		}
		private void AppendActionButton(ActionRecord actionRecord)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.actionTemplate, this.actionListParent);
			gameObject.SetActive(true);
			gameObject.GetComponentInChildren<TextMeshProUGUI>().text = actionRecord.Name;
			gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
			{
				this.SetGraph(actionRecord);
			});
		}
		private void SetGraph(ActionRecord actionRecord)
		{
			this.ClearGraph();
			this.textContent.text = ActionRecord.Dump(actionRecord);
		}
		private void ClearGraph()
		{
			this.textContent.text = "";
		}
		private void OnDebugBattleLogClicked(InputAction.CallbackContext obj)
		{
			if (base.IsVisible)
			{
				base.Hide();
				return;
			}
			base.Show();
		}
		[SerializeField]
		private ScrollRect actionListScrollRect;
		[SerializeField]
		private Transform actionListParent;
		[SerializeField]
		private GameObject actionTemplate;
		[SerializeField]
		private ScrollRect actionGraphScrollRect;
		[SerializeField]
		private Transform actionGraphParent;
		[SerializeField]
		private GameObject actionHeaderTemplate;
		[SerializeField]
		private TextMeshProUGUI textContent;
		[SerializeField]
		private TextMeshProUGUI infoText;
	}
}
