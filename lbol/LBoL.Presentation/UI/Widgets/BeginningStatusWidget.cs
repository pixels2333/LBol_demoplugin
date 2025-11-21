using System;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200003F RID: 63
	public class BeginningStatusWidget : MonoBehaviour
	{
		// Token: 0x0600041A RID: 1050 RVA: 0x0001088D File Offset: 0x0000EA8D
		private void Awake()
		{
			SimpleTooltipSource.CreateWithGeneralKey(this.healthIcon.gameObject, "StartGame.StartHp", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKey(this.goldIcon.gameObject, "StartGame.StartMoney", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x000108CC File Offset: 0x0000EACC
		public void Init(ManaGroup mana, int health, int gold)
		{
			this.manaLayout.transform.DestroyChildren();
			foreach (ManaColor manaColor in ManaColors.Colors)
			{
				for (int i = 0; i < mana.GetValue(manaColor); i++)
				{
					Object.Instantiate<BaseManaWidget>(this.manaPrefab, this.manaLayout.transform).SetBaseMana(manaColor);
				}
			}
			this.healthValue.text = health.ToString();
			this.goldValue.text = gold.ToString();
		}

		// Token: 0x040001E9 RID: 489
		[SerializeField]
		private GameObject manaLayout;

		// Token: 0x040001EA RID: 490
		[SerializeField]
		private TextMeshProUGUI healthValue;

		// Token: 0x040001EB RID: 491
		[SerializeField]
		private TextMeshProUGUI goldValue;

		// Token: 0x040001EC RID: 492
		[SerializeField]
		private Image healthIcon;

		// Token: 0x040001ED RID: 493
		[SerializeField]
		private Image goldIcon;

		// Token: 0x040001EE RID: 494
		[SerializeField]
		private BaseManaWidget manaPrefab;
	}
}
