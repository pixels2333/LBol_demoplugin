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
	public class ComplexRulesPanel : UiPanel, IInputActionHandler
	{
		public override async UniTask CustomLocalizationAsync()
		{
			Dictionary<string, ComplexRulesPanel.StringTableEntry> dictionary = await Localization.LoadFileAsync<ComplexRulesPanel.StringTableEntry>("Rule");
			this._stringTable = dictionary;
			foreach (ComplexRulesPanel.StringTableEntry stringTableEntry in this._stringTable.Values)
			{
				stringTableEntry.Description = StringDecorator.Decorate(stringTableEntry.Description);
			}
		}
		public override void OnLocaleChanged()
		{
			this.CustomLocalizationAsync();
			this.LoadRule();
		}
		protected override void OnShowing()
		{
			this.descriptionText.text = "";
			this._entityList.Clear();
			this.entityContent.DestroyChildren();
			this._canvasGroup.interactable = true;
			this.LoadRule();
			UiManager.PushActionHandler(this);
		}
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}
		private void Awake()
		{
			this.bgButton.onClick.AddListener(new UnityAction(base.Hide));
			this.rightContent.sizeDelta = Vector2.zero;
			this.titleTemplate.SetActive(false);
			this.cardTemplate.gameObject.SetActive(false);
			this.exhibitTemplate.gameObject.SetActive(false);
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}
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
		[SerializeField]
		private Transform titleRoot;
		[SerializeField]
		private RectTransform rightContent;
		[SerializeField]
		private RectTransform entityContent;
		[SerializeField]
		private TextMeshProUGUI descriptionText;
		[SerializeField]
		private GameObject titleTemplate;
		[SerializeField]
		private CardWidget cardTemplate;
		[SerializeField]
		private MuseumExhibitWidget exhibitTemplate;
		[SerializeField]
		private Button bgButton;
		[SerializeField]
		private Vector2 entitySpacing;
		private Dictionary<string, ComplexRulesPanel.StringTableEntry> _stringTable;
		private readonly List<GameObject> _entityList = new List<GameObject>();
		private CanvasGroup _canvasGroup;
		[UsedImplicitly]
		private sealed class StringTableEntry
		{
			public string Name;
			public string Description;
		}
	}
}
