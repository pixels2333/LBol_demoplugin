using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.ConfigData;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class CreditsPanel : UiPanel, IInputActionHandler
	{
		private void Awake()
		{
			IEnumerable<ValueTuple<Type, CardConfig>> enumerable = Enumerable.Where<ValueTuple<Type, CardConfig>>(Library.EnumerateCardTypes(), ([TupleElementNames(new string[] { "cardType", "config" })] ValueTuple<Type, CardConfig> c) => c.Item2.DebugLevel == 0);
			List<string> list = Enumerable.ToList<string>(Enumerable.Select<ValueTuple<Type, CardConfig>, string>(enumerable, ([TupleElementNames(new string[] { "cardType", "config" })] ValueTuple<Type, CardConfig> c) => c.Item2.Illustrator));
			foreach (ValueTuple<Type, CardConfig> valueTuple in enumerable)
			{
				if (valueTuple.Item2.SubIllustrator != null)
				{
					foreach (string text in valueTuple.Item2.SubIllustrator)
					{
						list.Add(text);
					}
				}
			}
			List<string> list2 = Enumerable.ToList<string>(Enumerable.Select<IGrouping<string, string>, string>(Enumerable.OrderByDescending<IGrouping<string, string>, int>(Enumerable.GroupBy<string, string>(list, (string x) => x), (IGrouping<string, string> x) => Enumerable.Count<string>(x)), (IGrouping<string, string> x) => x.Key));
			bool flag = false;
			foreach (string text2 in list2)
			{
				if (text2 != null && !text2.EndsWith("(Sketch)") && !text2.EndsWith("(Community)") && text2 != "乌鸦" && text2 != "射命丸文" && text2 != "AliothStudio")
				{
					if (flag)
					{
						Object.Instantiate<TextMeshProUGUI>(this.illuName, this.illuLayout).text = text2;
					}
					else
					{
						this.illuName.text = text2;
						flag = true;
					}
				}
			}
			this.returnButton.onClick.AddListener(new UnityAction(base.Hide));
		}
		protected override void OnShowing()
		{
			UiManager.PushActionHandler(this);
			this.tipText.text = string.Format("UI.SkipHint".Localize(true), UiManager.GetSkipString());
			this.tipText.gameObject.SetActive(true);
			this._closer = base.StartCoroutine("Closer");
		}
		private IEnumerator Closer()
		{
			yield return new WaitForSeconds(5f);
			this.tipText.gameObject.SetActive(false);
			yield break;
		}
		protected override void OnShown()
		{
			this._mainSequence = DOTween.Sequence();
			this._mainSequence.Join(this.content.DOAnchorPos(new Vector2(-this.totalLength, 0f), this.totalDuration, false).From(Vector2.zero, true, false).SetEase(Ease.Linear));
			float num = this.totalLength / 3840f;
			this._mainSequence.Join(DOTween.To(() => this.scrollBg.uvRect, delegate(Rect r)
			{
				this.scrollBg.uvRect = r;
			}, new Rect(num, 0f, 1f, 1f), this.totalDuration).From(new Rect(0f, 0f, 1f, 1f), true, false).SetEase(Ease.Linear));
			this._mainSequence.Join(DOTween.To(() => this.bg1.uvRect, delegate(Rect r)
			{
				this.bg1.uvRect = r;
			}, new Rect(num / 3f, 0f, 1f, 1f), this.totalDuration).From(new Rect(0f, 0f, 1f, 1f), true, false).SetEase(Ease.Linear));
			this._mainSequence.Join(DOTween.To(() => this.bg2.uvRect, delegate(Rect r)
			{
				this.bg2.uvRect = r;
			}, new Rect(num / 2f, 0f, 1f, 1f), this.totalDuration).From(new Rect(0f, 0f, 1f, 1f), true, false).SetEase(Ease.Linear));
			this._mainSequence.SetUpdate(true).SetDelay(2f).SetLink(base.gameObject)
				.OnComplete(delegate
				{
					if (this._closer != null)
					{
						base.StopCoroutine(this._closer);
					}
					this.tipText.text = "UI.SideCloseHint".Localize(true);
					this.tipText.gameObject.SetActive(true);
				});
		}
		protected override void OnHided()
		{
			Sequence mainSequence = this._mainSequence;
			if (mainSequence != null)
			{
				mainSequence.Kill(false);
			}
			this._mainSequence = null;
			this.content.anchoredPosition = Vector2.zero;
			this.scrollBg.uvRect = new Rect(0f, 0f, 1f, 1f);
			this.bg1.uvRect = new Rect(0f, 0f, 1f, 1f);
			this.bg2.uvRect = new Rect(0f, 0f, 1f, 1f);
		}
		protected override void OnHiding()
		{
			UiManager.PopActionHandler(this);
		}
		void IInputActionHandler.OnCancel()
		{
			this.returnButton.onClick.Invoke();
		}
		void IInputActionHandler.BeginSkipDialog()
		{
			if (this._mainSequence != null)
			{
				this._mainSequence.timeScale = 5f;
			}
		}
		void IInputActionHandler.EndSkipDialog()
		{
			if (this._mainSequence != null)
			{
				this._mainSequence.timeScale = 1f;
			}
		}
		[SerializeField]
		private RectTransform content;
		[SerializeField]
		private Button returnButton;
		[SerializeField]
		private TextMeshProUGUI tipText;
		[SerializeField]
		private RawImage scrollBg;
		[SerializeField]
		private RawImage bg1;
		[SerializeField]
		private RawImage bg2;
		[SerializeField]
		private float totalDuration = 40f;
		[SerializeField]
		private float totalLength;
		[SerializeField]
		private RectTransform illuLayout;
		[SerializeField]
		private TextMeshProUGUI illuName;
		private Sequence _mainSequence;
		private Coroutine _closer;
	}
}
