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
	public class StartStatusWidget : MonoBehaviour
	{
		private void Awake()
		{
			SimpleTooltipSource.CreateWithGeneralKey(this.healthIcon.gameObject, "StartGame.StartHp", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			SimpleTooltipSource.CreateWithGeneralKey(this.goldIcon.gameObject, "StartGame.StartMoney", null).WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.manaPrefab.gameObject.SetActive(false);
		}
		public void SetCharacter(PlayerUnit player)
		{
			this.characterName.text = player.Name;
			this.characterSubName.text = player.Title;
			this.healthValue.text = player.Config.MaxHp.ToString();
			this.goldValue.text = player.Config.InitialMoney.ToString();
		}
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
		[SerializeField]
		private TextMeshProUGUI characterName;
		[SerializeField]
		private TextMeshProUGUI characterSubName;
		[SerializeField]
		private TextMeshProUGUI healthValue;
		[SerializeField]
		private TextMeshProUGUI goldValue;
		[SerializeField]
		private Image healthIcon;
		[SerializeField]
		private Image goldIcon;
		[SerializeField]
		private List<GameObject> characterRateList;
		[SerializeField]
		private GameObject manaLayout;
		[SerializeField]
		private BaseManaWidget manaPrefab;
	}
}
