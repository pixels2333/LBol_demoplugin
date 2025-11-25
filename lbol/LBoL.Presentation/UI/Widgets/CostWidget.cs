using System;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using TMPro;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class CostWidget : MonoBehaviour
	{
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
		private static bool IsCostUp(ManaGroup cost, ManaGroup baseCose)
		{
			int num = cost.Amount.CompareTo(baseCose.Amount);
			return num > 0 || (num == 0 && cost.Any < baseCose.Any);
		}
		private static bool IsCostDown(ManaGroup cost, ManaGroup baseCose)
		{
			int num = cost.Amount.CompareTo(baseCose.Amount);
			return num <= 0 && (num != 0 || cost.Any > baseCose.Any);
		}
		public RectTransform rectTransform;
		[SerializeField]
		private GameObject costParent;
		[SerializeField]
		private TextMeshProUGUI costTmp;
		[SerializeField]
		private GameObject costTempChange;
		[SerializeField]
		private GameObject costUp;
		[SerializeField]
		private GameObject costDown;
		[SerializeField]
		private GameObject loyaltyParent;
		[SerializeField]
		private TextMeshProUGUI loyaltyTmp;
		private bool _showLoyalty;
	}
}
