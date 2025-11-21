using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Presentation.InputSystemExtend;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200008F RID: 143
	public class ComplexRulesPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x0600078C RID: 1932 RVA: 0x000234FC File Offset: 0x000216FC
		public override async UniTask CustomLocalizationAsync()
		{
			Dictionary<string, ComplexRulesPanel.StringTableEntry> dictionary = await Localization.LoadFileAsync<ComplexRulesPanel.StringTableEntry>("Rule");
			this._stringTable = dictionary;
			foreach (ComplexRulesPanel.StringTableEntry stringTableEntry in this._stringTable.Values)
			{
				stringTableEntry.Description = StringDecorator.Decorate(stringTableEntry.Description);
			}
		}

		// Token: 0x0600078D RID: 1933 RVA: 0x0002353F File Offset: 0x0002173F
		public override void OnLocaleChanged()
		{
			this.CustomLocalizationAsync();
			this.LoadRule();
		}

		// Token: 0x0600078E RID: 1934 RVA: 0x0002354E File Offset: 0x0002174E
		protected override void OnShowing()
		{
			this.descriptionText.text = "";
			this._entityList.Clear();
			this.entityContent.DestroyChildren();
			this._canvasGroup.interactable = true;
			this.LoadRule();
			UiManager.PushActionHandler(this);
		}

		// Token: 0x0600078F RID: 1935 RVA: 0x0002358E File Offset: 0x0002178E
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x06000790 RID: 1936 RVA: 0x000235A2 File Offset: 0x000217A2
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x000235AC File Offset: 0x000217AC
		private void Awake()
		{
			this.bgButton.onClick.AddListener(new UnityAction(base.Hide));
			this.rightContent.sizeDelta = Vector2.zero;
			this.titleTemplate.SetActive(false);
			this.cardTemplate.gameObject.SetActive(false);
			this.exhibitTemplate.gameObject.SetActive(false);
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x00023620 File Offset: 0x00021820
		private void LoadRule()
		{
			this.titleRoot.transform.DestroyChildren();
			using (IEnumerator<RuleConfig> enumerator = RuleConfig.AllConfig().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					RuleConfig config = enumerator.Current;
					bool flag = RuleConfig.AllConfig().IndexOf(config) != 0;
					GameObject gameObject = Object.Instantiate<GameObject>(this.titleTemplate, this.titleRoot);
					gameObject.SetActive(true);
					ComplexRulesPanel.StringTableEntry stringTableEntry;
					this._stringTable.TryGetValue(config.Id, ref stringTableEntry);
					gameObject.GetComponentInChildren<TextMeshProUGUI>().text = ((stringTableEntry != null) ? stringTableEntry.Name : "");
					gameObject.GetComponent<Button>().onClick.AddListener(delegate
					{
						this.SetDescription(config);
					});
					if (!flag)
					{
						gameObject.AddComponent<GamepadNavigationOrigin>();
					}
				}
			}
			RuleConfig ruleConfig = RuleConfig.FromId("General");
			if (ruleConfig != null)
			{
				this.SetDescription(ruleConfig);
			}
		}

		// Token: 0x06000793 RID: 1939 RVA: 0x00023728 File Offset: 0x00021928
		private void SetDescription(RuleConfig ruleConfig)
		{
			ComplexRulesPanel.StringTableEntry stringTableEntry;
			this._stringTable.TryGetValue(ruleConfig.Id, ref stringTableEntry);
			this.descriptionText.text = ((stringTableEntry != null) ? stringTableEntry.Description : "");
			this._entityList.Clear();
			this.entityContent.DestroyChildren();
			foreach (string text in ruleConfig.CardIds)
			{
				CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardTemplate, this.entityContent);
				cardWidget.Card = Library.CreateCard(text);
				cardWidget.GetComponent<ShowingCard>().SetScale(1f, 1.2f);
				cardWidget.gameObject.SetActive(true);
				this._entityList.Add(cardWidget.gameObject);
			}
			foreach (string text2 in ruleConfig.ExhibitIds)
			{
				MuseumExhibitWidget museumExhibitWidget = Object.Instantiate<MuseumExhibitWidget>(this.exhibitTemplate, this.entityContent);
				museumExhibitWidget.Exhibit = Library.CreateExhibit(text2);
				museumExhibitWidget.gameObject.SetActive(true);
				this._entityList.Add(museumExhibitWidget.gameObject);
			}
			this.SetContentSize();
		}

		// Token: 0x06000794 RID: 1940 RVA: 0x00023880 File Offset: 0x00021A80
		private void SetContentSize()
		{
			RectTransform component = this.descriptionText.GetComponent<RectTransform>();
			float num;
			float num2;
			float num3;
			float num4;
			this.descriptionText.margin.Deconstruct(out num, out num2, out num3, out num4);
			float num5 = num;
			float num6 = num2;
			float num7 = num3;
			float num8 = num4;
			this.descriptionText.GetPreferredValues(component.rect.width - num5 - num7, component.rect.height - num6 - num8).Deconstruct(out num4, out num3);
			float num9 = num3 + num6 + num8;
			component.sizeDelta = new Vector2(0f, num9);
			float num10 = 100f;
			int num11 = ((this._entityList.Count > 0) ? 1 : 0);
			float num12 = this.cardTemplate.GetComponent<RectTransform>().rect.height + this.entitySpacing.y;
			float width = this.entityContent.rect.width;
			foreach (GameObject gameObject in this._entityList)
			{
				RectTransform component2 = gameObject.GetComponent<RectTransform>();
				if (num10 + component2.rect.width + this.entitySpacing.x > width)
				{
					num10 = 100f;
					num11++;
				}
				component2.anchoredPosition = new Vector2(num10 + component2.rect.width / 2f, (float)(-(float)(num11 - 1)) * num12 - component2.rect.height / 2f);
				num10 += component2.rect.width + this.entitySpacing.x;
			}
			float num13 = num12 * (float)num11;
			this.entityContent.sizeDelta = new Vector2(0f, num13);
			this.rightContent.sizeDelta = new Vector2(0f, num9 + num13);
		}

		// Token: 0x040004F9 RID: 1273
		[SerializeField]
		private Transform titleRoot;

		// Token: 0x040004FA RID: 1274
		[SerializeField]
		private RectTransform rightContent;

		// Token: 0x040004FB RID: 1275
		[SerializeField]
		private RectTransform entityContent;

		// Token: 0x040004FC RID: 1276
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x040004FD RID: 1277
		[SerializeField]
		private GameObject titleTemplate;

		// Token: 0x040004FE RID: 1278
		[SerializeField]
		private CardWidget cardTemplate;

		// Token: 0x040004FF RID: 1279
		[SerializeField]
		private MuseumExhibitWidget exhibitTemplate;

		// Token: 0x04000500 RID: 1280
		[SerializeField]
		private Button bgButton;

		// Token: 0x04000501 RID: 1281
		[SerializeField]
		private Vector2 entitySpacing;

		// Token: 0x04000502 RID: 1282
		private Dictionary<string, ComplexRulesPanel.StringTableEntry> _stringTable;

		// Token: 0x04000503 RID: 1283
		private readonly List<GameObject> _entityList = new List<GameObject>();

		// Token: 0x04000504 RID: 1284
		private CanvasGroup _canvasGroup;

		// Token: 0x02000247 RID: 583
		[UsedImplicitly]
		private sealed class StringTableEntry
		{
			// Token: 0x04001075 RID: 4213
			public string Name;

			// Token: 0x04001076 RID: 4214
			public string Description;
		}
	}
}
