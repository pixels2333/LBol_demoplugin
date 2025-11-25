using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class HealthBar : MonoBehaviour
	{
		private void Awake()
		{
			this.blockImage.fillAmount = 0f;
			this.shieldImage.fillAmount = 0f;
			this.healthImage.fillAmount = 1f;
			this.shieldParent.gameObject.SetActive(false);
			this.blockParent.gameObject.SetActive(false);
		}
		private void OnDestroy()
		{
			DOTween.Kill(this, false);
		}
		public void SetHp(int hp, int maxHp)
		{
			this.healthTmp.text = string.Format("{0}/{1}", hp, maxHp);
			this._hp = hp;
			this._maxHp = maxHp;
		}
		public void SetShield(int shield, int block)
		{
			this.blockTmp.text = block.ToString();
			this.shieldTmp.text = shield.ToString();
			if (this._shield == 0 && shield > 0)
			{
				this.shieldCg.DOKill(true);
				this.shieldParent.gameObject.SetActive(true);
				this.shieldCg.DOFade(1f, 0.2f).SetUpdate(true);
			}
			else if (this._shield > 0 && shield == 0)
			{
				this.shieldCg.DOKill(true);
				this.shieldCg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(delegate
				{
					this.shieldParent.gameObject.SetActive(false);
				});
			}
			if (this._block == 0 && block > 0)
			{
				this.blockCg.DOKill(true);
				this.blockParent.gameObject.SetActive(true);
				this.blockCg.DOFade(1f, 0.2f).SetUpdate(true);
			}
			else if (this._block > 0 && block == 0)
			{
				this.blockCg.DOKill(true);
				this.blockCg.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(delegate
				{
					this.blockParent.gameObject.SetActive(false);
				});
			}
			this._shield = shield;
			this._block = block;
		}
		public void TweenHp(int hp, int maxHp, int shield, int block, bool instant = false)
		{
			int curHp = this._hp;
			float num = (float)hp / (float)maxHp;
			float num2 = 0.3f;
			if (1f - num > 0.3f)
			{
				num2 = 1f - num;
			}
			if (1f - num > 0.6f)
			{
				num2 = 0.6f;
			}
			float num3 = num;
			float num4 = 0f;
			float num5 = 0f;
			if (shield + block != 0)
			{
				float num6 = num2 * (float)(shield + block) / ((float)(shield + block) + 20f);
				float num7 = num6 * (float)shield / (float)(shield + block);
				float num8 = num6 * (float)block / (float)(shield + block);
				if (num6 > 1f - num)
				{
					num3 = 1f - num6;
				}
				num4 = num3 + num7;
				num5 = num4 + num8;
			}
			this.DOKill(true);
			if (instant)
			{
				this.healthImage.fillAmount = num3;
				this.shieldImage.fillAmount = num4;
				this.blockImage.fillAmount = num5;
				this.SetHp(hp, maxHp);
				this.SetShield(shield, block);
				return;
			}
			this.SetShield(shield, block);
			float lerp = 0f;
			DOTween.Sequence().Insert(0.1f, this.healthImage.DOFillAmount(num3, 0.2f)).Insert(0.05f, this.shieldImage.DOFillAmount(num4, 0.2f))
				.Insert(0f, this.blockImage.DOFillAmount(num5, 0.2f))
				.Insert(0f, DOTween.To(() => lerp, delegate(float v)
				{
					lerp = v;
					this.SetHp(v.Lerp((float)curHp, (float)hp).RoundToInt(), maxHp);
				}, 1f, 0.5f))
				.SetUpdate(true)
				.SetTarget(this);
		}
		[SerializeField]
		private Image healthImage;
		[SerializeField]
		private Image shieldImage;
		[SerializeField]
		private Image blockImage;
		[SerializeField]
		private TextMeshProUGUI healthTmp;
		[SerializeField]
		private TextMeshProUGUI shieldTmp;
		[SerializeField]
		private TextMeshProUGUI blockTmp;
		[SerializeField]
		private Transform shieldParent;
		[SerializeField]
		private Transform blockParent;
		[SerializeField]
		private CanvasGroup shieldCg;
		[SerializeField]
		private CanvasGroup blockCg;
		private int _hp;
		private int _maxHp;
		private int _shield;
		private int _block;
		private const float TweenTime = 0.2f;
		private const float MinPercentOfShieldAndBlock = 0.3f;
		private const float MaxPercentOfShieldAndBlock = 0.6f;
	}
}
