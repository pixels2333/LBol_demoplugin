using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000074 RID: 116
	public class StartStatusWidget : MonoBehaviour
	{
		// Token: 0x060005EF RID: 1519 RVA: 0x00019A38 File Offset: 0x00017C38
		private void Awake()
		{
			SimpleTooltipSource.CreateWithGeneralKey(this.healthIcon.gameObject, "StartGame.StartHp", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKey(this.goldIcon.gameObject, "StartGame.StartMoney", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.manaPrefab.gameObject.SetActive(false);
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00019A94 File Offset: 0x00017C94
		public void SetCharacter(PlayerUnit player)
		{
			this.characterName.text = player.Name;
			this.characterSubName.text = player.Title;
			this.healthValue.text = player.Config.MaxHp.ToString();
			this.goldValue.text = player.Config.InitialMoney.ToString();
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x00019B00 File Offset: 0x00017D00
		public void SetSetup(ManaGroup totalMana, int rate)
		{
			for (int i = 0; i < Enumerable.Count<GameObject>(this.characterRateList); i++)
			{
				this.characterRateList[i].SetActive(i < rate);
			}
			this.manaLayout.transform.DestroyChildren();
			foreach (ManaColor manaColor in ManaColors.Colors)
			{
				for (int j = 0; j < totalMana.GetValue(manaColor); j++)
				{
					BaseManaWidget baseManaWidget = Object.Instantiate<BaseManaWidget>(this.manaPrefab, this.manaLayout.transform);
					baseManaWidget.SetBaseMana(manaColor);
					baseManaWidget.gameObject.SetActive(true);
				}
			}
		}

		// Token: 0x040003A5 RID: 933
		[SerializeField]
		private TextMeshProUGUI characterName;

		// Token: 0x040003A6 RID: 934
		[SerializeField]
		private TextMeshProUGUI characterSubName;

		// Token: 0x040003A7 RID: 935
		[SerializeField]
		private TextMeshProUGUI healthValue;

		// Token: 0x040003A8 RID: 936
		[SerializeField]
		private TextMeshProUGUI goldValue;

		// Token: 0x040003A9 RID: 937
		[SerializeField]
		private Image healthIcon;

		// Token: 0x040003AA RID: 938
		[SerializeField]
		private Image goldIcon;

		// Token: 0x040003AB RID: 939
		[SerializeField]
		private List<GameObject> characterRateList;

		// Token: 0x040003AC RID: 940
		[SerializeField]
		private GameObject manaLayout;

		// Token: 0x040003AD RID: 941
		[SerializeField]
		private BaseManaWidget manaPrefab;
	}
}
