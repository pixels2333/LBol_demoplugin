using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Exhibits.Common;
using UnityEngine;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage3
{
	[AdventureInfo(WeighterType = typeof(BackgroundDancers.BackgroundDancersWeighter))]
	public sealed class BackgroundDancers : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			this._1Out10Hp = Mathf.CeilToInt((float)base.GameRun.Player.MaxHp * 0.09f);
			this._2Out10Hp = 2 * this._1Out10Hp;
			this._money = 100;
			this._maxHp = 4;
			this._power = 30;
			this._img2Alpha = 0;
			storage.SetValue("$hpLose", (float)this._1Out10Hp);
			storage.SetValue("$doubleHpLose", (float)this._2Out10Hp);
			storage.SetValue("$moneyGain", (float)this._money);
			storage.SetValue("$maxHpGain", (float)this._maxHp);
			storage.SetValue("$power", (float)this._power);
		}
		[RuntimeCommand("rollOptions", "")]
		[UsedImplicitly]
		public void RollOptions()
		{
			for (int i = 0; i < 6; i++)
			{
				this._optionTitles[i] = base.Storage.GetValue(string.Format("$option{0}Source", i + 1));
				this._optionDialogues[i] = base.Storage.GetValue(string.Format("$option{0}Dialogue", i + 1));
			}
			this._optionIndices = new UniqueRandomPool<int>(false)
			{
				{ 0, 1f },
				{ 1, 1f },
				{ 2, 1f },
				{ 3, 1f },
				{ 4, 1f },
				{ 5, 1f }
			}.SampleMany(base.GameRun.AdventureRng, 3, true);
			base.Storage.SetValue("$option1", this._optionTitles[this._optionIndices[0]]);
			base.Storage.SetValue("$option1Dialogue", this._optionDialogues[this._optionIndices[0]]);
			base.Storage.SetValue("$option2", this._optionTitles[this._optionIndices[1]]);
			base.Storage.SetValue("$option2Dialogue", this._optionDialogues[this._optionIndices[1]]);
			base.Storage.SetValue("$option3", this._optionTitles[this._optionIndices[2]]);
			base.Storage.SetValue("$option3Dialogue", this._optionDialogues[this._optionIndices[2]]);
			this.toolCards = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false), 2, false, false, null);
			base.Storage.SetValue("$reward2Rare", this.toolCards[0].Id);
			base.Storage.SetValue("$reward2Rare2", this.toolCards[1].Id);
			this.exhibitReward = base.Stage.GetSpecialAdventureExhibit();
			base.Storage.SetValue("$IsSentinel", this.exhibitReward.Config.IsSentinel);
			base.Storage.SetValue("$reward5Exhibit", this.exhibitReward.Config.Id);
			this.abilityCard = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(CardTypeWeightTable.OnlyAbility), 1, false, false, null);
			base.Storage.SetValue("$reward6Ability", this.abilityCard[0].Config.Id);
			for (int j = 0; j < 3; j++)
			{
				switch (this._optionIndices[j])
				{
				case 0:
				case 2:
				case 3:
					break;
				case 1:
					base.Storage.SetValue("$tip2Rare", (float)(j + 1));
					break;
				case 4:
					base.Storage.SetValue("$tip5Exhibit", (float)(j + 1));
					break;
				case 5:
					base.Storage.SetValue("$tip6Ability", (float)(j + 1));
					break;
				default:
					throw new ArgumentException();
				}
			}
		}
		[RuntimeCommand("selectOption", "")]
		[UsedImplicitly]
		public IEnumerator SelectOption(int index)
		{
			int selected = this._optionIndices[index - 1];
			switch (selected)
			{
			case 0:
				this.ForMoney();
				break;
			case 1:
				this.ForTechnique();
				break;
			case 2:
				this.ForStrength();
				break;
			case 3:
				this.ForPower();
				break;
			case 4:
				yield return this.ForExhibit();
				break;
			case 5:
				this.ForAbility();
				break;
			default:
				throw new ArgumentException();
			}
			UniqueRandomPool<int> uniqueRandomPool = new UniqueRandomPool<int>(false);
			for (int i = 0; i < 6; i++)
			{
				if (!Enumerable.Contains<int>(this._optionIndices, i))
				{
					uniqueRandomPool.Add(i, 1f);
				}
			}
			int num = uniqueRandomPool.Sample(base.GameRun.AdventureRng);
			this._optionIndices[index - 1] = num;
			base.Storage.SetValue("$option1", this._optionTitles[this._optionIndices[0]]);
			base.Storage.SetValue("$option1Dialogue", this._optionDialogues[this._optionIndices[0]]);
			base.Storage.SetValue("$option2", this._optionTitles[this._optionIndices[1]]);
			base.Storage.SetValue("$option2Dialogue", this._optionDialogues[this._optionIndices[1]]);
			base.Storage.SetValue("$option3", this._optionTitles[this._optionIndices[2]]);
			base.Storage.SetValue("$option3Dialogue", this._optionDialogues[this._optionIndices[2]]);
			if (selected == 1)
			{
				this.toolCards = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.EliteCard, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyTool, false), 2, false, false, null);
				base.Storage.SetValue("$reward2Rare", this.toolCards[0].Id);
				base.Storage.SetValue("$reward2Rare2", this.toolCards[1].Id);
			}
			if (selected == 4)
			{
				this.exhibitReward = base.Stage.GetSpecialAdventureExhibit();
				base.Storage.SetValue("$IsSentinel", this.exhibitReward.Config.IsSentinel);
				base.Storage.SetValue("$reward5Exhibit", this.exhibitReward.Config.Id);
			}
			if (selected == 5)
			{
				this.abilityCard = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(CardTypeWeightTable.OnlyAbility), 1, false, false, null);
				base.Storage.SetValue("$reward6Ability", this.abilityCard[0].Config.Id);
			}
			base.Storage.SetValue("$tip2Rare", 0f);
			base.Storage.SetValue("$tip5Exhibit", 0f);
			base.Storage.SetValue("$tip6Ability", 0f);
			for (int j = 0; j < 3; j++)
			{
				switch (this._optionIndices[j])
				{
				case 0:
				case 2:
				case 3:
					break;
				case 1:
					base.Storage.SetValue("$tip2Rare", (float)(j + 1));
					break;
				case 4:
					base.Storage.SetValue("$tip5Exhibit", (float)(j + 1));
					break;
				case 5:
					base.Storage.SetValue("$tip6Ability", (float)(j + 1));
					break;
				default:
					throw new ArgumentException();
				}
			}
			yield break;
		}
		private void ChangeAlpha()
		{
			if (this._img2Alpha < 100)
			{
				this._img2Alpha += 20;
				this._img2Alpha = Mathf.Clamp(this._img2Alpha, 0, 100);
			}
			Debug.Log(this._img2Alpha);
		}
		private void ForMoney()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.GainMoney(this._money, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn
			});
		}
		private void ForTechnique()
		{
			base.GameRun.Damage(this._2Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.AddDeckCards(this.toolCards, false, null);
		}
		private void ForStrength()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.GainMaxHp(this._maxHp, true, true);
		}
		private void ForPower()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.GainPower(this._power, false);
		}
		private IEnumerator ForExhibit()
		{
			base.GameRun.Damage(this._2Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				yield break;
			}
			yield return base.GameRun.GainExhibitRunner(this.exhibitReward, true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn,
				Index = 1
			});
			Exhibit exhibit = this.exhibitReward;
			if ((exhibit is Cookie || exhibit is Taozi || exhibit is Pingguo || exhibit is Putao) && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.CookieInBackdoor);
			}
			yield break;
		}
		private void ForAbility()
		{
			base.GameRun.Damage(this._1Out10Hp, DamageType.HpLose, true, true, this);
			if (base.GameRun.Status == GameRunStatus.Failure)
			{
				return;
			}
			base.GameRun.AddDeckCards(this.abilityCard, false, null);
		}
		private int _1Out10Hp;
		private int _money;
		private int _maxHp;
		private int _2Out10Hp;
		private int _power;
		private int _img2Alpha;
		private readonly string[] _optionTitles = new string[6];
		private readonly string[] _optionDialogues = new string[6];
		private int[] _optionIndices;
		private Card[] toolCards;
		private Exhibit exhibitReward;
		private Card[] abilityCard;
		private class BackgroundDancersWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Player.Hp >= gameRun.Player.MaxHp / 2) ? 1 : 0);
			}
		}
	}
}
