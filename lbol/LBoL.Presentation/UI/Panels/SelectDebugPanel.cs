using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000B2 RID: 178
	public class SelectDebugPanel : UiPanel<BattleAdvTestStation>
	{
		// Token: 0x17000197 RID: 407
		// (get) Token: 0x060009ED RID: 2541 RVA: 0x00032232 File Offset: 0x00030432
		// (set) Token: 0x060009EE RID: 2542 RVA: 0x0003223A File Offset: 0x0003043A
		private BattleAdvTestStation Station { get; set; }

		// Token: 0x060009EF RID: 2543 RVA: 0x00032244 File Offset: 0x00030444
		public void Awake()
		{
			this.buttonTemplate.gameObject.SetActive(false);
			this.selectNormal.onClick.AddListener(new UnityAction(this.SelectNormal));
			this.selectRealName.onClick.AddListener(new UnityAction(this.SelectRealName));
			this.selectAdventure.onClick.AddListener(new UnityAction(this.SelectAdventure));
			this.selectNormal.enabled = false;
		}

		// Token: 0x060009F0 RID: 2544 RVA: 0x000322C4 File Offset: 0x000304C4
		private void CreateEnemyButtons()
		{
			using (IEnumerator<EnemyGroupEntry> enumerator = Library.EnumerateEnemyGroupEntries().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					EnemyGroupEntry entry = enumerator.Current;
					EnemyGroupConfig enemyGroupConfig = EnemyGroupConfig.FromId(entry.Id);
					if (enemyGroupConfig != null && !enemyGroupConfig.IsSub)
					{
						SelectDebugPanel.SelectionType selectionType = this._selectionType;
						if (selectionType != SelectDebugPanel.SelectionType.Normal)
						{
							if (selectionType == SelectDebugPanel.SelectionType.RealName)
							{
								if (enemyGroupConfig.EnemyType == EnemyType.Normal)
								{
									continue;
								}
							}
						}
						else if (enemyGroupConfig.EnemyType != EnemyType.Normal)
						{
							continue;
						}
						Button button = Object.Instantiate<Button>(this.buttonTemplate, this.layout);
						string name = enemyGroupConfig.Name;
						button.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = name;
						button.onClick.AddListener(delegate
						{
							this.Station.SetEnemy(entry);
							this.Hide();
						});
						button.gameObject.SetActive(true);
					}
				}
			}
		}

		// Token: 0x060009F1 RID: 2545 RVA: 0x000323BC File Offset: 0x000305BC
		private void CreateAdventureButtons()
		{
			foreach (Type type in Library.EnumerateAdventureTypes())
			{
				Adventure adv = Library.CreateAdventure(type);
				Button button = Object.Instantiate<Button>(this.buttonTemplate, this.layout);
				button.transform.GetComponentInChildren<TextMeshProUGUI>().text = adv.HostName + ": " + adv.Title;
				button.onClick.AddListener(delegate
				{
					this.Station.SetAdventure(adv);
					this.Hide();
				});
				button.gameObject.SetActive(true);
			}
		}

		// Token: 0x060009F2 RID: 2546 RVA: 0x00032484 File Offset: 0x00030684
		private void SelectNormal()
		{
			this._selectionType = SelectDebugPanel.SelectionType.Normal;
			this.selectNormal.enabled = false;
			this.selectRealName.enabled = true;
			this.selectAdventure.enabled = true;
			this.layout.DestroyChildren();
			this.CreateEnemyButtons();
		}

		// Token: 0x060009F3 RID: 2547 RVA: 0x000324C2 File Offset: 0x000306C2
		private void SelectRealName()
		{
			this._selectionType = SelectDebugPanel.SelectionType.RealName;
			this.selectNormal.enabled = true;
			this.selectRealName.enabled = false;
			this.selectAdventure.enabled = true;
			this.layout.DestroyChildren();
			this.CreateEnemyButtons();
		}

		// Token: 0x060009F4 RID: 2548 RVA: 0x00032500 File Offset: 0x00030700
		private void SelectAdventure()
		{
			this._selectionType = SelectDebugPanel.SelectionType.Adventure;
			this.selectNormal.enabled = true;
			this.selectRealName.enabled = true;
			this.selectAdventure.enabled = false;
			this.layout.DestroyChildren();
			this.CreateAdventureButtons();
		}

		// Token: 0x060009F5 RID: 2549 RVA: 0x0003253E File Offset: 0x0003073E
		protected override void OnShowing(BattleAdvTestStation payload)
		{
			this.Station = payload;
			this.SelectNormal();
		}

		// Token: 0x060009F6 RID: 2550 RVA: 0x0003254D File Offset: 0x0003074D
		protected override void OnHided()
		{
			this.Station = null;
			this.layout.DestroyChildren();
		}

		// Token: 0x04000760 RID: 1888
		private SelectDebugPanel.SelectionType _selectionType;

		// Token: 0x04000761 RID: 1889
		[SerializeField]
		private Transform layout;

		// Token: 0x04000762 RID: 1890
		[SerializeField]
		private Button buttonTemplate;

		// Token: 0x04000763 RID: 1891
		[SerializeField]
		private Button selectNormal;

		// Token: 0x04000764 RID: 1892
		[SerializeField]
		private Button selectRealName;

		// Token: 0x04000765 RID: 1893
		[SerializeField]
		private Button selectAdventure;

		// Token: 0x020002B7 RID: 695
		private enum SelectionType
		{
			// Token: 0x0400120A RID: 4618
			Normal,
			// Token: 0x0400120B RID: 4619
			RealName,
			// Token: 0x0400120C RID: 4620
			Adventure
		}
	}
}
