using System;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class BeginningStatusWidget : MonoBehaviour
	{
		private void Awake()
		{
			SimpleTooltipSource.CreateWithGeneralKey(this.healthIcon.gameObject, "StartGame.StartHp", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKey(this.goldIcon.gameObject, "StartGame.StartMoney", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
		}
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
		[SerializeField]
		private GameObject manaLayout;
		[SerializeField]
		private TextMeshProUGUI healthValue;
		[SerializeField]
		private TextMeshProUGUI goldValue;
		[SerializeField]
		private Image healthIcon;
		[SerializeField]
		private Image goldIcon;
		[SerializeField]
		private BaseManaWidget manaPrefab;
	}
}
