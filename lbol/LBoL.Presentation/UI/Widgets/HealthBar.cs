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
	// Token: 0x0200005B RID: 91
	public class HealthBar : MonoBehaviour
	{
		// Token: 0x0600051B RID: 1307 RVA: 0x00015C0C File Offset: 0x00013E0C
		private void Awake()
		{
			this.blockImage.fillAmount = 0f;
			this.shieldImage.fillAmount = 0f;
			this.healthImage.fillAmount = 1f;
			this.shieldParent.gameObject.SetActive(false);
			this.blockParent.gameObject.SetActive(false);
		}

		// Token: 0x0600051C RID: 1308 RVA: 0x00015C6B File Offset: 0x00013E6B
		private void OnDestroy()
		{
			DOTween.Kill(this, false);
		}

		// Token: 0x0600051D RID: 1309 RVA: 0x00015C75 File Offset: 0x00013E75
		public void SetHp(int hp, int maxHp)
		{
			this.healthTmp.text = string.Format("{0}/{1}", hp, maxHp);
			this._hp = hp;
			this._maxHp = maxHp;
		}

		// Token: 0x0600051E RID: 1310 RVA: 0x00015CA8 File Offset: 0x00013EA8
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

		// Token: 0x0600051F RID: 1311 RVA: 0x00015E04 File Offset: 0x00014004
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

		// Token: 0x040002DA RID: 730
		[SerializeField]
		private Image healthImage;

		// Token: 0x040002DB RID: 731
		[SerializeField]
		private Image shieldImage;

		// Token: 0x040002DC RID: 732
		[SerializeField]
		private Image blockImage;

		// Token: 0x040002DD RID: 733
		[SerializeField]
		private TextMeshProUGUI healthTmp;

		// Token: 0x040002DE RID: 734
		[SerializeField]
		private TextMeshProUGUI shieldTmp;

		// Token: 0x040002DF RID: 735
		[SerializeField]
		private TextMeshProUGUI blockTmp;

		// Token: 0x040002E0 RID: 736
		[SerializeField]
		private Transform shieldParent;

		// Token: 0x040002E1 RID: 737
		[SerializeField]
		private Transform blockParent;

		// Token: 0x040002E2 RID: 738
		[SerializeField]
		private CanvasGroup shieldCg;

		// Token: 0x040002E3 RID: 739
		[SerializeField]
		private CanvasGroup blockCg;

		// Token: 0x040002E4 RID: 740
		private int _hp;

		// Token: 0x040002E5 RID: 741
		private int _maxHp;

		// Token: 0x040002E6 RID: 742
		private int _shield;

		// Token: 0x040002E7 RID: 743
		private int _block;

		// Token: 0x040002E8 RID: 744
		private const float TweenTime = 0.2f;

		// Token: 0x040002E9 RID: 745
		private const float MinPercentOfShieldAndBlock = 0.3f;

		// Token: 0x040002EA RID: 746
		private const float MaxPercentOfShieldAndBlock = 0.6f;
	}
}
