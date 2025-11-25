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
	public class SelectDebugPanel : UiPanel<BattleAdvTestStation>
	{
		private BattleAdvTestStation Station { get; set; }
		public void Awake()
		{
			this.buttonTemplate.gameObject.SetActive(false);
			this.selectNormal.onClick.AddListener(new UnityAction(this.SelectNormal));
			this.selectRealName.onClick.AddListener(new UnityAction(this.SelectRealName));
			this.selectAdventure.onClick.AddListener(new UnityAction(this.SelectAdventure));
			this.selectNormal.enabled = false;
		}
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
		private void SelectNormal()
		{
			this._selectionType = SelectDebugPanel.SelectionType.Normal;
			this.selectNormal.enabled = false;
			this.selectRealName.enabled = true;
			this.selectAdventure.enabled = true;
			this.layout.DestroyChildren();
			this.CreateEnemyButtons();
		}
		private void SelectRealName()
		{
			this._selectionType = SelectDebugPanel.SelectionType.RealName;
			this.selectNormal.enabled = true;
			this.selectRealName.enabled = false;
			this.selectAdventure.enabled = true;
			this.layout.DestroyChildren();
			this.CreateEnemyButtons();
		}
		private void SelectAdventure()
		{
			this._selectionType = SelectDebugPanel.SelectionType.Adventure;
			this.selectNormal.enabled = true;
			this.selectRealName.enabled = true;
			this.selectAdventure.enabled = false;
			this.layout.DestroyChildren();
			this.CreateAdventureButtons();
		}
		protected override void OnShowing(BattleAdvTestStation payload)
		{
			this.Station = payload;
			this.SelectNormal();
		}
		protected override void OnHided()
		{
			this.Station = null;
			this.layout.DestroyChildren();
		}
		private SelectDebugPanel.SelectionType _selectionType;
		[SerializeField]
		private Transform layout;
		[SerializeField]
		private Button buttonTemplate;
		[SerializeField]
		private Button selectNormal;
		[SerializeField]
		private Button selectRealName;
		[SerializeField]
		private Button selectAdventure;
		private enum SelectionType
		{
			Normal,
			RealName,
			Adventure
		}
	}
}
