using System;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200004E RID: 78
	public class CostWidget : MonoBehaviour
	{
		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060004A7 RID: 1191 RVA: 0x000132A5 File Offset: 0x000114A5
		// (set) Token: 0x060004A8 RID: 1192 RVA: 0x000132B0 File Offset: 0x000114B0
		public bool ShowLoyalty
		{
			get
			{
				return this._showLoyalty;
			}
			set
			{
				if (this._showLoyalty != value)
				{
					this._showLoyalty = value;
					if (this._showLoyalty)
					{
						this.costParent.SetActive(false);
						this.loyaltyParent.SetActive(true);
						return;
					}
					this.costParent.SetActive(true);
					this.loyaltyParent.SetActive(false);
				}
			}
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x00013308 File Offset: 0x00011508
		public void SetCost(Card card, bool hasEffect = true)
		{
			this.ShowLoyalty = card.Summoned;
			if (card.Summoned)
			{
				this.loyaltyTmp.text = card.Loyalty.ToString();
				return;
			}
			if (!card.IsXCost)
			{
				ManaGroup cost = card.Cost;
				this.costTmp.text = UiUtils.ManaGroupToText(cost, GameMaster.IsLoopOrder);
				this.costUp.SetActive(hasEffect && CostWidget.IsCostUp(card.Cost, card.ConfigCost));
				this.costDown.SetActive(hasEffect && CostWidget.IsCostDown(card.Cost, card.ConfigCost));
				this.costTempChange.SetActive(hasEffect && card.Cost + card.AuraCost != card.BaseCost);
				return;
			}
			ManaGroup xcostRequiredMana = card.XCostRequiredMana;
			if (xcostRequiredMana.IsEmpty)
			{
				this.costTmp.text = UiUtils.XCostText;
				return;
			}
			this.costTmp.text = UiUtils.XCostText + UiUtils.ManaGroupToText(xcostRequiredMana, GameMaster.IsLoopOrder);
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x0001341C File Offset: 0x0001161C
		private static bool IsCostUp(ManaGroup cost, ManaGroup baseCose)
		{
			int num = cost.Amount.CompareTo(baseCose.Amount);
			return num > 0 || (num == 0 && cost.Any < baseCose.Any);
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x00013464 File Offset: 0x00011664
		private static bool IsCostDown(ManaGroup cost, ManaGroup baseCose)
		{
			int num = cost.Amount.CompareTo(baseCose.Amount);
			return num <= 0 && (num != 0 || cost.Any > baseCose.Any);
		}

		// Token: 0x04000274 RID: 628
		public RectTransform rectTransform;

		// Token: 0x04000275 RID: 629
		[SerializeField]
		private GameObject costParent;

		// Token: 0x04000276 RID: 630
		[SerializeField]
		private TextMeshProUGUI costTmp;

		// Token: 0x04000277 RID: 631
		[SerializeField]
		private GameObject costTempChange;

		// Token: 0x04000278 RID: 632
		[SerializeField]
		private GameObject costUp;

		// Token: 0x04000279 RID: 633
		[SerializeField]
		private GameObject costDown;

		// Token: 0x0400027A RID: 634
		[SerializeField]
		private GameObject loyaltyParent;

		// Token: 0x0400027B RID: 635
		[SerializeField]
		private TextMeshProUGUI loyaltyTmp;

		// Token: 0x0400027C RID: 636
		private bool _showLoyalty;
	}
}
