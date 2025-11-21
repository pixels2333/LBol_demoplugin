using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x02000089 RID: 137
	public class BattleNotifier : UiPanel
	{
		// Token: 0x17000130 RID: 304
		// (get) Token: 0x060006FD RID: 1789 RVA: 0x0001FE20 File Offset: 0x0001E020
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x0001FE24 File Offset: 0x0001E024
		public void Awake()
		{
			this._battleCanvasGroup = this.battleRoot.GetComponent<CanvasGroup>();
			this._turnCanvasGroup = this.turnRoot.GetComponent<CanvasGroup>();
			this.battleRoot.gameObject.SetActive(false);
			this.turnRoot.gameObject.SetActive(false);
			this.playerTitle.gameObject.SetActive(false);
			this.enemyTitle.gameObject.SetActive(false);
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x0001FE98 File Offset: 0x0001E098
		public YieldInstruction ShowBattleStart()
		{
			this.battleRoot.gameObject.SetActive(true);
			return DOTween.Sequence().Append(this.battleRoot.DOScale(Vector3.one, 0.2f).From(new Vector3(1.5f, 1.5f, 1.5f), true, false)).Join(this._battleCanvasGroup.DOFade(1f, 0.2f).From(0f, true, false))
				.AppendInterval(0.5f)
				.Append(this.battleRoot.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f))
				.Join(this._battleCanvasGroup.DOFade(0f, 0.2f))
				.OnComplete(delegate
				{
					this.battleRoot.gameObject.SetActive(false);
				})
				.SetLink(base.gameObject)
				.SetUpdate(true)
				.WaitForCompletion();
		}

		// Token: 0x06000700 RID: 1792 RVA: 0x0001FF8C File Offset: 0x0001E18C
		public YieldInstruction ShowPlayerTurn(int counter, bool isExtra)
		{
			this.turnRoot.gameObject.SetActive(true);
			this._turnCanvasGroup.alpha = 0f;
			this.playerTitle.gameObject.SetActive(true);
			this.roundCounter.text = (isExtra ? "Game.ExtraTurn".Localize(true) : string.Format("Game.RoundCounter".Localize(true), counter));
			if (isExtra)
			{
				this.extraTurnParticle.GetComponent<ParticleSystem>().Play();
				AudioManager.PlayUi("ExtraTurn", false);
			}
			else
			{
				AudioManager.PlayUi("PlayerTurn", false);
			}
			return this.DoShowTurn();
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x00020030 File Offset: 0x0001E230
		public YieldInstruction ShowEnemyTurn(int counter)
		{
			this.turnRoot.gameObject.SetActive(true);
			this._turnCanvasGroup.alpha = 0f;
			this.enemyTitle.gameObject.SetActive(true);
			this.roundCounter.text = string.Format("Game.RoundCounter".Localize(true), counter);
			return this.DoShowTurn();
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x00020098 File Offset: 0x0001E298
		private YieldInstruction DoShowTurn()
		{
			return DOTween.Sequence().Append(this._turnCanvasGroup.DOFade(1f, 0.2f)).Join(this.turnIcon.DOScale(1f, 0.3f).From(1.2f, true, false))
				.Join(this.turnIcon.DOLocalRotate(new Vector3(0f, 0f, -40f), 0.5f, RotateMode.Fast).From(Vector3.zero, true, false).SetEase(Ease.OutSine))
				.Join(this.turnBg.DOScale(new Vector3(1f, 0f, 1f), 0.5f).From(new Vector3(0.8f, 1f, 1f), true, false))
				.AppendInterval(0.5f)
				.Join(this.turnIcon.DOScale(1.2f, 0.3f).From(1f, true, false))
				.Join(this._turnCanvasGroup.DOFade(0f, 0.3f))
				.OnComplete(delegate
				{
					this.playerTitle.gameObject.SetActive(false);
					this.enemyTitle.gameObject.SetActive(false);
					this.turnRoot.gameObject.SetActive(false);
				})
				.SetLink(base.gameObject)
				.SetUpdate(true)
				.WaitForCompletion();
		}

		// Token: 0x0400047F RID: 1151
		[SerializeField]
		private Transform battleRoot;

		// Token: 0x04000480 RID: 1152
		[SerializeField]
		private Transform turnRoot;

		// Token: 0x04000481 RID: 1153
		[SerializeField]
		private Transform turnIcon;

		// Token: 0x04000482 RID: 1154
		[SerializeField]
		private Transform turnBg;

		// Token: 0x04000483 RID: 1155
		[SerializeField]
		private TextMeshProUGUI playerTitle;

		// Token: 0x04000484 RID: 1156
		[SerializeField]
		private TextMeshProUGUI enemyTitle;

		// Token: 0x04000485 RID: 1157
		[SerializeField]
		private TextMeshProUGUI roundCounter;

		// Token: 0x04000486 RID: 1158
		[SerializeField]
		private GameObject extraTurnParticle;

		// Token: 0x04000487 RID: 1159
		private CanvasGroup _battleCanvasGroup;

		// Token: 0x04000488 RID: 1160
		private CanvasGroup _turnCanvasGroup;
	}
}
