using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class StartSetupWidget : MonoBehaviour
	{
		public UltimateSkill Us
		{
			get
			{
				return this._us;
			}
		}
		public Button DeckButton
		{
			get
			{
				return this.deckButton;
			}
		}
		public Exhibit Exhibit
		{
			get
			{
				return this.exhibitWidget.Exhibit;
			}
		}
		private void Awake()
		{
			this.Initialize();
		}
		private void Initialize()
		{
			if (!this._initialized)
			{
				this._usTsSource = this.usIcon.gameObject.AddComponent<UltimateSkillTooltipSource>();
				this._usTsSource.enabled = false;
				this._initialized = true;
			}
		}
		public void Set(UltimateSkill us, Exhibit exhibit)
		{
			this.Initialize();
			this._us = us;
			this.usIcon.sprite = ((us != null) ? ResourcesHelper.TryGetSprite<UltimateSkill>(us.Id) : null);
			if (this.usIcon.sprite == null)
			{
				this.usIcon.sprite = this.fallbackSprite;
			}
			if (us != null)
			{
				this._usTsSource.enabled = true;
				this._usTsSource.Skill = us;
				using (List<TextMeshProUGUI>.Enumerator enumerator = this.spellText.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						TextMeshProUGUI textMeshProUGUI = enumerator.Current;
						textMeshProUGUI.text = "StartGame.SkillSpellCard".LocalizeFormat(new object[] { us.Config.PowerCost });
					}
					goto IL_00CA;
				}
			}
			this._usTsSource.enabled = false;
			IL_00CA:
			this.exhibitWidget.Exhibit = exhibit;
		}
		public void SelectThisSkill(bool active)
		{
			this.cursor.SetActive(active);
			foreach (TextMeshProUGUI textMeshProUGUI in this.textList)
			{
				textMeshProUGUI.gameObject.SetActive(active);
			}
			foreach (TextMeshProUGUI textMeshProUGUI2 in this.inactiveTextList)
			{
				textMeshProUGUI2.gameObject.SetActive(!active);
			}
		}
		[SerializeField]
		private Button button;
		[SerializeField]
		private Image usIcon;
		[SerializeField]
		private ExhibitWidget exhibitWidget;
		[SerializeField]
		private GameObject cursor;
		[SerializeField]
		private List<TextMeshProUGUI> spellText;
		[SerializeField]
		private List<TextMeshProUGUI> textList;
		[SerializeField]
		private List<TextMeshProUGUI> inactiveTextList;
		[SerializeField]
		private Button deckButton;
		[SerializeField]
		private Sprite fallbackSprite;
		private UltimateSkill _us;
		private UltimateSkillTooltipSource _usTsSource;
		private bool _initialized;
	}
}
