using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Panels
{
	public class BattleNotifier : UiPanel
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
			this._battleCanvasGroup = this.battleRoot.GetComponent<CanvasGroup>();
			this._turnCanvasGroup = this.turnRoot.GetComponent<CanvasGroup>();
			this.battleRoot.gameObject.SetActive(false);
			this.turnRoot.gameObject.SetActive(false);
			this.playerTitle.gameObject.SetActive(false);
			this.enemyTitle.gameObject.SetActive(false);
		}
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
		public YieldInstruction ShowEnemyTurn(int counter)
		{
			this.turnRoot.gameObject.SetActive(true);
			this._turnCanvasGroup.alpha = 0f;
			this.enemyTitle.gameObject.SetActive(true);
			this.roundCounter.text = string.Format("Game.RoundCounter".Localize(true), counter);
			return this.DoShowTurn();
		}
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
		[SerializeField]
		private Transform battleRoot;
		[SerializeField]
		private Transform turnRoot;
		[SerializeField]
		private Transform turnIcon;
		[SerializeField]
		private Transform turnBg;
		[SerializeField]
		private TextMeshProUGUI playerTitle;
		[SerializeField]
		private TextMeshProUGUI enemyTitle;
		[SerializeField]
		private TextMeshProUGUI roundCounter;
		[SerializeField]
		private GameObject extraTurnParticle;
		private CanvasGroup _battleCanvasGroup;
		private CanvasGroup _turnCanvasGroup;
	}
}
