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
	// Token: 0x02000092 RID: 146
	public class DebugBattleLogPanel : UiPanel
	{
		// Token: 0x17000147 RID: 327
		// (get) Token: 0x060007AC RID: 1964 RVA: 0x000240FD File Offset: 0x000222FD
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Topmost;
			}
		}

		// Token: 0x060007AD RID: 1965 RVA: 0x00024100 File Offset: 0x00022300
		private void Awake()
		{
			this.actionTemplate.gameObject.SetActive(false);
			this.actionHeaderTemplate.gameObject.SetActive(false);
		}

		// Token: 0x060007AE RID: 1966 RVA: 0x00024124 File Offset: 0x00022324
		public override UniTask InitializeAsync()
		{
			InputAction battleLog = UiManager.Instance.BattleLog;
			battleLog.performed += new Action<InputAction.CallbackContext>(this.OnDebugBattleLogClicked);
			this.infoText.text = "Battle Log (press " + battleLog.GetBindingDisplayString((InputBinding.DisplayStringOptions)0, null) + " to close)";
			this.SetActionButtons(ActionRecord.ActionRecords);
			ActionRecord.ActionResolvedHandler = (Action<ActionRecord>)Delegate.Combine(ActionRecord.ActionResolvedHandler, new Action<ActionRecord>(this.OnActionResolved));
			return UniTask.CompletedTask;
		}

		// Token: 0x060007AF RID: 1967 RVA: 0x000241A0 File Offset: 0x000223A0
		private void OnDestroy()
		{
			UiManager.Instance.BattleLog.performed -= new Action<InputAction.CallbackContext>(this.OnDebugBattleLogClicked);
			ActionRecord.ActionResolvedHandler = (Action<ActionRecord>)Delegate.Remove(ActionRecord.ActionResolvedHandler, new Action<ActionRecord>(this.OnActionResolved));
		}

		// Token: 0x060007B0 RID: 1968 RVA: 0x000241DD File Offset: 0x000223DD
		protected override void OnEnterBattle()
		{
			this.SetActionButtons(Enumerable.Empty<ActionRecord>());
		}

		// Token: 0x060007B1 RID: 1969 RVA: 0x000241EA File Offset: 0x000223EA
		protected override void OnLeaveBattle()
		{
		}

		// Token: 0x060007B2 RID: 1970 RVA: 0x000241EC File Offset: 0x000223EC
		protected override void OnShowing()
		{
			this.actionListScrollRect.ScrollToBottom();
			if (ActionRecord.ActionRecords.Count > 0)
			{
				this.SetGraph(Enumerable.Last<ActionRecord>(ActionRecord.ActionRecords));
			}
		}

		// Token: 0x060007B3 RID: 1971 RVA: 0x00024218 File Offset: 0x00022418
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

		// Token: 0x060007B4 RID: 1972 RVA: 0x00024280 File Offset: 0x00022480
		private void OnActionResolved(ActionRecord actionRecord)
		{
			this.AppendActionButton(actionRecord);
			this.SetGraph(actionRecord);
			this.actionListScrollRect.ScrollToBottom();
		}

		// Token: 0x060007B5 RID: 1973 RVA: 0x0002429C File Offset: 0x0002249C
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

		// Token: 0x060007B6 RID: 1974 RVA: 0x00024306 File Offset: 0x00022506
		private void SetGraph(ActionRecord actionRecord)
		{
			this.ClearGraph();
			this.textContent.text = ActionRecord.Dump(actionRecord);
		}

		// Token: 0x060007B7 RID: 1975 RVA: 0x0002431F File Offset: 0x0002251F
		private void ClearGraph()
		{
			this.textContent.text = "";
		}

		// Token: 0x060007B8 RID: 1976 RVA: 0x00024331 File Offset: 0x00022531
		private void OnDebugBattleLogClicked(InputAction.CallbackContext obj)
		{
			if (base.IsVisible)
			{
				base.Hide();
				return;
			}
			base.Show();
		}

		// Token: 0x04000513 RID: 1299
		[SerializeField]
		private ScrollRect actionListScrollRect;

		// Token: 0x04000514 RID: 1300
		[SerializeField]
		private Transform actionListParent;

		// Token: 0x04000515 RID: 1301
		[SerializeField]
		private GameObject actionTemplate;

		// Token: 0x04000516 RID: 1302
		[SerializeField]
		private ScrollRect actionGraphScrollRect;

		// Token: 0x04000517 RID: 1303
		[SerializeField]
		private Transform actionGraphParent;

		// Token: 0x04000518 RID: 1304
		[SerializeField]
		private GameObject actionHeaderTemplate;

		// Token: 0x04000519 RID: 1305
		[SerializeField]
		private TextMeshProUGUI textContent;

		// Token: 0x0400051A RID: 1306
		[SerializeField]
		private TextMeshProUGUI infoText;
	}
}
