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
	// Token: 0x02000091 RID: 145
	public class CreditsPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x0600079B RID: 1947 RVA: 0x00023AF8 File Offset: 0x00021CF8
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

		// Token: 0x0600079C RID: 1948 RVA: 0x00023D24 File Offset: 0x00021F24
		protected override void OnShowing()
		{
			UiManager.PushActionHandler(this);
			this.tipText.text = string.Format("UI.SkipHint".Localize(true), UiManager.GetSkipString());
			this.tipText.gameObject.SetActive(true);
			this._closer = base.StartCoroutine("Closer");
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x00023D79 File Offset: 0x00021F79
		private IEnumerator Closer()
		{
			yield return new WaitForSeconds(5f);
			this.tipText.gameObject.SetActive(false);
			yield break;
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x00023D88 File Offset: 0x00021F88
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

		// Token: 0x0600079F RID: 1951 RVA: 0x00023F6C File Offset: 0x0002216C
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

		// Token: 0x060007A0 RID: 1952 RVA: 0x0002400E File Offset: 0x0002220E
		protected override void OnHiding()
		{
			UiManager.PopActionHandler(this);
		}

		// Token: 0x060007A1 RID: 1953 RVA: 0x00024016 File Offset: 0x00022216
		void IInputActionHandler.OnCancel()
		{
			this.returnButton.onClick.Invoke();
		}

		// Token: 0x060007A2 RID: 1954 RVA: 0x00024028 File Offset: 0x00022228
		void IInputActionHandler.BeginSkipDialog()
		{
			if (this._mainSequence != null)
			{
				this._mainSequence.timeScale = 5f;
			}
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x00024042 File Offset: 0x00022242
		void IInputActionHandler.EndSkipDialog()
		{
			if (this._mainSequence != null)
			{
				this._mainSequence.timeScale = 1f;
			}
		}

		// Token: 0x04000507 RID: 1287
		[SerializeField]
		private RectTransform content;

		// Token: 0x04000508 RID: 1288
		[SerializeField]
		private Button returnButton;

		// Token: 0x04000509 RID: 1289
		[SerializeField]
		private TextMeshProUGUI tipText;

		// Token: 0x0400050A RID: 1290
		[SerializeField]
		private RawImage scrollBg;

		// Token: 0x0400050B RID: 1291
		[SerializeField]
		private RawImage bg1;

		// Token: 0x0400050C RID: 1292
		[SerializeField]
		private RawImage bg2;

		// Token: 0x0400050D RID: 1293
		[SerializeField]
		private float totalDuration = 40f;

		// Token: 0x0400050E RID: 1294
		[SerializeField]
		private float totalLength;

		// Token: 0x0400050F RID: 1295
		[SerializeField]
		private RectTransform illuLayout;

		// Token: 0x04000510 RID: 1296
		[SerializeField]
		private TextMeshProUGUI illuName;

		// Token: 0x04000511 RID: 1297
		private Sequence _mainSequence;

		// Token: 0x04000512 RID: 1298
		private Coroutine _closer;
	}
}
