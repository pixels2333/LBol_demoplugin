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
	public class SpellPanel : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		public void Awake()
		{
			this.root.gameObject.SetActive(false);
			this.spellName.gameObject.SetActive(false);
			foreach (SpellDeclareWidget spellDeclareWidget in Resources.LoadAll<SpellDeclareWidget>("UI/Panels/SpellDeclare"))
			{
				this.declareTemplates.Add(spellDeclareWidget.name, spellDeclareWidget);
			}
		}
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
		private void InternalSpellDeclare()
		{
			base.StopCoroutine("CoInternalSpellDeclare");
			base.StartCoroutine("CoInternalSpellDeclare");
		}
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
		[SerializeField]
		private Transform root;
		[SerializeField]
		private Image portrait;
		[SerializeField]
		private Image portraitShadow;
		[SerializeField]
		private TextMeshProUGUI spellName;
		[SerializeField]
		private Image speedLine1;
		[SerializeField]
		private Image speedLine2;
		[SerializeField]
		private Image trail;
		[SerializeField]
		private Image background;
		[SerializeField]
		private ParticleSystem particleRoot;
		[SerializeField]
		private Transform spellRoot;
		private const string Path = "UI/Panels/SpellDeclare";
		private readonly Dictionary<string, SpellDeclareWidget> declareTemplates = new Dictionary<string, SpellDeclareWidget>();
		private string _currentKey;
		private IDictionary<string, SpellPanel.Entry> _l10nTable;
		private sealed class Entry
		{
			[UsedImplicitly]
			public string Title { get; set; }
			[UsedImplicitly]
			public string Name { get; set; }
		}
	}
}
