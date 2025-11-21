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
	// Token: 0x02000073 RID: 115
	public class StartSetupWidget : MonoBehaviour
	{
		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060005E7 RID: 1511 RVA: 0x00019838 File Offset: 0x00017A38
		public UltimateSkill Us
		{
			get
			{
				return this._us;
			}
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060005E8 RID: 1512 RVA: 0x00019840 File Offset: 0x00017A40
		public Button DeckButton
		{
			get
			{
				return this.deckButton;
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060005E9 RID: 1513 RVA: 0x00019848 File Offset: 0x00017A48
		public Exhibit Exhibit
		{
			get
			{
				return this.exhibitWidget.Exhibit;
			}
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00019855 File Offset: 0x00017A55
		private void Awake()
		{
			this.Initialize();
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x0001985D File Offset: 0x00017A5D
		private void Initialize()
		{
			if (!this._initialized)
			{
				this._usTsSource = this.usIcon.gameObject.AddComponent<UltimateSkillTooltipSource>();
				this._usTsSource.enabled = false;
				this._initialized = true;
			}
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00019890 File Offset: 0x00017A90
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

		// Token: 0x060005ED RID: 1517 RVA: 0x00019984 File Offset: 0x00017B84
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

		// Token: 0x04000399 RID: 921
		[SerializeField]
		private Button button;

		// Token: 0x0400039A RID: 922
		[SerializeField]
		private Image usIcon;

		// Token: 0x0400039B RID: 923
		[SerializeField]
		private ExhibitWidget exhibitWidget;

		// Token: 0x0400039C RID: 924
		[SerializeField]
		private GameObject cursor;

		// Token: 0x0400039D RID: 925
		[SerializeField]
		private List<TextMeshProUGUI> spellText;

		// Token: 0x0400039E RID: 926
		[SerializeField]
		private List<TextMeshProUGUI> textList;

		// Token: 0x0400039F RID: 927
		[SerializeField]
		private List<TextMeshProUGUI> inactiveTextList;

		// Token: 0x040003A0 RID: 928
		[SerializeField]
		private Button deckButton;

		// Token: 0x040003A1 RID: 929
		[SerializeField]
		private Sprite fallbackSprite;

		// Token: 0x040003A2 RID: 930
		private UltimateSkill _us;

		// Token: 0x040003A3 RID: 931
		private UltimateSkillTooltipSource _usTsSource;

		// Token: 0x040003A4 RID: 932
		private bool _initialized;
	}
}
