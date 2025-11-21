using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000BA RID: 186
	public class SpellPanel : UiPanel
	{
		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06000AA6 RID: 2726 RVA: 0x000354D5 File Offset: 0x000336D5
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x000354D8 File Offset: 0x000336D8
		public void Awake()
		{
			this.root.gameObject.SetActive(false);
			this.spellName.gameObject.SetActive(false);
			foreach (SpellDeclareWidget spellDeclareWidget in Resources.LoadAll<SpellDeclareWidget>("UI/Panels/SpellDeclare"))
			{
				this.declareTemplates.Add(spellDeclareWidget.name, spellDeclareWidget);
			}
		}

		// Token: 0x06000AA8 RID: 2728 RVA: 0x00035538 File Offset: 0x00033738
		public override async UniTask CustomLocalizationAsync()
		{
			try
			{
				Dictionary<string, SpellPanel.Entry> dictionary = await Localization.LoadFileAsync<SpellPanel.Entry>("Spell");
				this._l10nTable = dictionary;
			}
			catch (Exception ex)
			{
				Debug.LogError("[SpellPanel] localization failed: " + ex.Message);
				this._l10nTable = new Dictionary<string, SpellPanel.Entry>();
			}
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x0003557C File Offset: 0x0003377C
		public void SetSpellNameByKey(string configKey, string spellKey)
		{
			this._currentKey = spellKey;
			SpellPanel.Entry entry;
			if (this._l10nTable == null || !this._l10nTable.TryGetValue(configKey, ref entry))
			{
				entry = new SpellPanel.Entry();
			}
			SpellPanel.Entry entry2 = entry;
			if (entry2.Name == null)
			{
				entry2.Name = "<Empty>";
			}
			if (entry.Title != null)
			{
				this.spellName.text = "UI.SpellTitled".LocalizeFormat(new object[] { entry.Title, entry.Name });
				return;
			}
			this.spellName.text = "UI.SpellUntitled".LocalizeFormat(new object[] { entry.Name });
		}

		// Token: 0x06000AAA RID: 2730 RVA: 0x00035620 File Offset: 0x00033820
		public void CallSpellDeclare(Sprite targetSprite, Color[] colors, float x = 0f, float y = 0f, float s = 1f)
		{
			this.portrait.sprite = targetSprite;
			this.portraitShadow.sprite = targetSprite;
			this.trail.color = colors[1];
			this.speedLine1.color = colors[0];
			this.speedLine2.color = colors[1];
			this.portraitShadow.color = colors[2];
			ParticleSystem[] componentsInChildren = this.particleRoot.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].main.startColor = colors[3];
			}
			Transform transform = this.portrait.transform;
			transform.localPosition = new Vector3(x, y, 0f);
			transform.localScale = new Vector3(s, s, 1f);
			this.portraitShadow.transform.localScale = new Vector3(s, s, 1f);
			this.InternalSpellDeclare();
		}

		// Token: 0x06000AAB RID: 2731 RVA: 0x00035718 File Offset: 0x00033918
		private void InternalSpellDeclare()
		{
			base.StopCoroutine("CoInternalSpellDeclare");
			base.StartCoroutine("CoInternalSpellDeclare");
		}

		// Token: 0x06000AAC RID: 2732 RVA: 0x00035731 File Offset: 0x00033931
		private IEnumerator CoInternalSpellDeclare()
		{
			this.spellName.gameObject.SetActive(true);
			this.spellName.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 0.6f, false).From(new Vector2(1000f, 0f), true, false);
			this.spellName.DOFade(1f, 0.2f).From(0f, true, false);
			if (this._currentKey != null && this.declareTemplates.ContainsKey(this._currentKey))
			{
				SpellDeclareWidget spellDeclareWidget = Object.Instantiate<SpellDeclareWidget>(this.declareTemplates[this._currentKey], this.spellRoot);
				spellDeclareWidget.DoAnimationIn();
				Object.Destroy(spellDeclareWidget.gameObject, 3f);
				yield return new WaitForSeconds(2f);
			}
			else
			{
				this.root.gameObject.SetActive(true);
				this.root.DOKill(false);
				this.root.GetComponent<CanvasGroup>().DOFade(1f, 0f);
				this.portrait.DOKill(false);
				this.portraitShadow.DOKill(false);
				Vector2 vector = new Vector2(this.portrait.transform.localPosition.x, this.portrait.transform.localPosition.y);
				this.portrait.GetComponent<RectTransform>().DOAnchorPos(new Vector2(200f, -500f) + vector, 1.5f, false).From(new Vector2(1000f, -1000f), true, false)
					.SetEase(Ease.OutCubic);
				this.portraitShadow.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, -300f) + vector, 1.5f, false).From(new Vector2(1000f, -1000f), true, false)
					.SetEase(Ease.OutCubic);
				this.background.GetComponent<RectTransform>().DOAnchorPos(new Vector2(1920f, 0f), 8f, false).From(new Vector2(-1920f, 0f), true, false)
					.SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear);
				this.speedLine1.GetComponent<RectTransform>().DOAnchorPos(new Vector2(5120f, 0f), 0.5f, false).From(Vector2.zero, true, false)
					.SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear);
				this.speedLine2.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-4096f, 0f), 0.5f, false).From(Vector2.zero, true, false)
					.SetLoops(-1, LoopType.Restart)
					.SetEase(Ease.Linear);
				this.trail.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0f, 5000f), 0.5f, false).From(new Vector2(0f, -1000f), true, false);
				this.root.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).From(1f, true, false)
					.SetDelay(1.5f);
				this.particleRoot.Play();
				yield return new WaitForSeconds(2f);
				this.background.GetComponent<RectTransform>().DOKill(false);
				this.speedLine1.GetComponent<RectTransform>().DOKill(false);
				this.speedLine2.GetComponent<RectTransform>().DOKill(false);
				this.root.gameObject.SetActive(false);
			}
			this.spellName.gameObject.SetActive(false);
			yield break;
		}

		// Token: 0x0400080F RID: 2063
		[SerializeField]
		private Transform root;

		// Token: 0x04000810 RID: 2064
		[SerializeField]
		private Image portrait;

		// Token: 0x04000811 RID: 2065
		[SerializeField]
		private Image portraitShadow;

		// Token: 0x04000812 RID: 2066
		[SerializeField]
		private TextMeshProUGUI spellName;

		// Token: 0x04000813 RID: 2067
		[SerializeField]
		private Image speedLine1;

		// Token: 0x04000814 RID: 2068
		[SerializeField]
		private Image speedLine2;

		// Token: 0x04000815 RID: 2069
		[SerializeField]
		private Image trail;

		// Token: 0x04000816 RID: 2070
		[SerializeField]
		private Image background;

		// Token: 0x04000817 RID: 2071
		[SerializeField]
		private ParticleSystem particleRoot;

		// Token: 0x04000818 RID: 2072
		[SerializeField]
		private Transform spellRoot;

		// Token: 0x04000819 RID: 2073
		private const string Path = "UI/Panels/SpellDeclare";

		// Token: 0x0400081A RID: 2074
		private readonly Dictionary<string, SpellDeclareWidget> declareTemplates = new Dictionary<string, SpellDeclareWidget>();

		// Token: 0x0400081B RID: 2075
		private string _currentKey;

		// Token: 0x0400081C RID: 2076
		private IDictionary<string, SpellPanel.Entry> _l10nTable;

		// Token: 0x020002CE RID: 718
		private sealed class Entry
		{
			// Token: 0x17000488 RID: 1160
			// (get) Token: 0x06001728 RID: 5928 RVA: 0x0006787D File Offset: 0x00065A7D
			// (set) Token: 0x06001729 RID: 5929 RVA: 0x00067885 File Offset: 0x00065A85
			[UsedImplicitly]
			public string Title { get; set; }

			// Token: 0x17000489 RID: 1161
			// (get) Token: 0x0600172A RID: 5930 RVA: 0x0006788E File Offset: 0x00065A8E
			// (set) Token: 0x0600172B RID: 5931 RVA: 0x00067896 File Offset: 0x00065A96
			[UsedImplicitly]
			public string Name { get; set; }
		}
	}
}
